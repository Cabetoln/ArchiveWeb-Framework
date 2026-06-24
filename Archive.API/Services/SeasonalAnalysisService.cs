using Archive.API.DTOs;
using Archive.API.Models;
using Archive.API.Repositories;

namespace Archive.API.Services;

public class SeasonalAnalysisService : ISeasonalAnalysisService
{
    private static readonly string[] MonthNames =
    [
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];

    private readonly IItemRepository _items;

    public SeasonalAnalysisService(IItemRepository items) => _items = items;

    public Task<SeasonalAnalysisStatusResponse> GetPendingStatusAsync() => GetStatusAsync();

    public async Task<SeasonalInsightResponse?> GetInsightAsync(Guid productId)
    {
        var product = await _items.GetByIdAsync(productId);
        if (product is null) return null;

        var history = (await _items.GetPriceHistoryAsync(productId))
            .OrderBy(p => p.RecordedAt)
            .ToList();

        if (history.Count <= 3)
        {
            if (!ShouldSimulateHistory(product.Id))
            {
                return new SeasonalInsightResponse(
                    product.Id,
                    HasEnoughHistory: false,
                    HasSimulatedHistory: false,
                    CurrentPrice: product.CurrentPrice,
                    CurrentMonthAverage: 0m,
                    DifferencePercentage: 0m,
                    Status: "Sem dados suficientes",
                    Recommendation: "Não há histórico suficiente para emitir uma análise sazonal.",
                    BestDiscountMonth: null,
                    MonthlyPatterns: Array.Empty<SeasonalPatternResponse>());
            }

            history = CreateSyntheticHistory(product);
            return BuildInsight(product, history, hasEnoughHistory: false, isSimulated: true);
        }

        return BuildInsight(product, history, hasEnoughHistory: true, isSimulated: false);
    }

    public async Task<SeasonalAnalysisStatusResponse> ProcessPendingAnalysisAsync()
    {
        var pending = await _items.GetSeasonalAnalysisPendingAsync();
        if (pending)
            await _items.SetSeasonalAnalysisPendingAsync(false);

        return new SeasonalAnalysisStatusResponse(false);
    }

    private async Task<SeasonalAnalysisStatusResponse> GetStatusAsync()
    {
        var pending = await _items.GetSeasonalAnalysisPendingAsync();
        return new SeasonalAnalysisStatusResponse(pending);
    }

    private static SeasonalInsightResponse BuildInsight(Product product, List<PriceHistory> history, bool hasEnoughHistory, bool isSimulated)
    {
        var overallAverage = history.Average(p => p.Price);
        var patterns = BuildMonthlyPatterns(history, overallAverage);
        var currentMonth = DateTime.UtcNow.Month;
        var currentPattern = patterns.FirstOrDefault(p => p.Month == currentMonth && p.HasData);
        var currentMonthAverage = currentPattern?.AveragePrice ?? overallAverage;

        var differencePercentage = currentMonthAverage > 0
            ? Math.Round((product.CurrentPrice - currentMonthAverage) / currentMonthAverage * 100m, 2)
            : 0m;

        var status = GetStatus(differencePercentage);
        var recommendation = GetRecommendation(status, isSimulated);
        var bestDiscountMonth = BuildBestDiscountMonth(patterns, overallAverage);

        return new SeasonalInsightResponse(
            product.Id,
            HasEnoughHistory: hasEnoughHistory,
            HasSimulatedHistory: isSimulated,
            CurrentPrice: product.CurrentPrice,
            CurrentMonthAverage: Math.Round(currentMonthAverage, 2),
            DifferencePercentage: differencePercentage,
            Status: status,
            Recommendation: recommendation,
            BestDiscountMonth: bestDiscountMonth,
            MonthlyPatterns: patterns);
    }

    private static string GetStatus(decimal diff)
    {
        if (diff <= -5m) return "Em baixa";
        if (diff >= 5m) return "Em alta";
        return "Estável";
    }

    private static string GetRecommendation(string status, bool isSimulated)
    {
        var base_ = status switch
        {
            "Em baixa" => "Bom momento para considerar a compra.",
            "Em alta"  => "Aguarde uma janela mais favorável.",
            _          => "Acompanhe a evolução de preço.",
        };

        return isSimulated ? base_ + " (baseado em dados simulados de demonstração)." : base_;
    }

    private static BestDiscountMonthResponse? BuildBestDiscountMonth(IReadOnlyList<SeasonalPatternResponse> patterns, decimal overallAverage)
    {
        var best = patterns.Where(p => p.HasData).OrderBy(p => p.AveragePrice).FirstOrDefault();
        if (best is null || best.AveragePrice is null) return null;

        var discountPercentage = overallAverage > 0
            ? Math.Round((best.AveragePrice.Value - overallAverage) / overallAverage * 100m, 2)
            : 0m;

        return new BestDiscountMonthResponse(
            Month: best.Month,
            MonthName: best.MonthName,
            Season: best.Season,
            DiscountPercentage: discountPercentage,
            IsDiscountPeriod: discountPercentage <= -5m,
            Insight: discountPercentage <= -5m
                ? $"Melhor janela provável de desconto: {best.MonthName}."
                : $"Melhor mês de menor preço esperado: {best.MonthName}."
        );
    }

    private static IReadOnlyList<SeasonalPatternResponse> BuildMonthlyPatterns(IEnumerable<PriceHistory> history, decimal overallAverage)
    {
        var grouped = history
            .GroupBy(p => p.RecordedAt.Month)
            .ToDictionary(g => g.Key, g => new { Average = g.Average(p => p.Price), Count = g.Count() });

        var patterns = new List<SeasonalPatternResponse>(12);

        for (var month = 1; month <= 12; month++)
        {
            if (grouped.TryGetValue(month, out var data))
            {
                var average = Math.Round(data.Average, 2);
                var discount = overallAverage > 0
                    ? Math.Round((average - overallAverage) / overallAverage * 100m, 2)
                    : 0m;

                patterns.Add(new SeasonalPatternResponse(
                    Month: month, MonthName: MonthNames[month - 1], Season: GetSeason(month),
                    AveragePrice: average, DiscountPercentage: discount,
                    IsDiscountPeriod: discount <= -5m, HasData: true));
            }
            else
            {
                patterns.Add(new SeasonalPatternResponse(
                    Month: month, MonthName: MonthNames[month - 1], Season: GetSeason(month),
                    AveragePrice: null, DiscountPercentage: null,
                    IsDiscountPeriod: false, HasData: false));
            }
        }

        return patterns;
    }

    private static List<PriceHistory> CreateSyntheticHistory(Product product)
    {
        var step = Math.Max(1m, Math.Round(product.CurrentPrice * 0.1m, 2));
        var now = DateTime.UtcNow;

        return Enumerable.Range(1, 4)
            .Select(offset => new PriceHistory
            {
                ProductId  = product.Id,
                Price      = Math.Max(1m, product.CurrentPrice - step * offset),
                Currency   = product.Currency,
                RecordedAt = now.AddMonths(-offset),
                Source     = "synthetic"
            })
            .ToList();
    }

    private static bool ShouldSimulateHistory(Guid productId)
    {
        var bytes = productId.ToByteArray();
        var seed = bytes.Aggregate(0, (current, value) => current + value);
        return seed % 2 == 0;
    }

    private static string GetSeason(int month) => month switch
    {
        12 or 1 or 2 => "Verão",
        3 or 4 or 5  => "Outono",
        6 or 7 or 8  => "Inverno",
        9 or 10 or 11 => "Primavera",
        _ => "Desconhecido",
    };
}
