using System.Text.Json;
using System.Text.Json.Serialization;

namespace Archive.Games;

/// <summary>Uma oferta já resolvida da CheapShark, com o nome da loja anexado.</summary>
public record CheapSharkDeal(
    string Title,
    string DealId,
    string StoreName,
    decimal SalePrice,
    decimal NormalPrice,
    decimal SavingsPercent,
    string? Thumb,
    string? RatingText);

/// <summary>
/// <b>Padrão Facade.</b> Oferece uma interface única e simples
/// (<see cref="GetDealsAsync"/>) sobre o subsistema da API pública da CheapShark,
/// escondendo do resto do plugin toda a complexidade: dois endpoints distintos
/// (<c>/stores</c> e <c>/deals</c>), construção de URL, desserialização JSON e a
/// junção que resolve <c>storeID → nome da loja</c>. O scraper (ponto flexível
/// <c>IPriceScraper</c>) conversa apenas com esta fachada.
/// </summary>
public class CheapSharkClient(HttpClient http, IConfiguration config)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    private readonly string _baseUrl =
        (config["CheapShark:BaseUrl"] ?? "https://www.cheapshark.com/api/1.0").TrimEnd('/');

    /// <summary>
    /// Retorna as ofertas em destaque já com o nome da loja resolvido — uma única
    /// chamada esconde as duas requisições e a junção entre elas.
    /// </summary>
    public async Task<IReadOnlyList<CheapSharkDeal>> GetDealsAsync(CancellationToken ct = default)
    {
        var storeMap = await GetStoreMapAsync(ct);

        var sortBy = config["CheapShark:SortBy"] ?? "Deal Rating";
        var onSaleOnly = !bool.TryParse(config["CheapShark:OnSaleOnly"], out var v) || v;
        var pageSize = int.TryParse(config["CheapShark:PageSize"], out var p) ? p : 20;

        var url = $"{_baseUrl}/deals?pageSize={pageSize}" +
                  $"&sortBy={Uri.EscapeDataString(sortBy)}" +
                  (onSaleOnly ? "&onSale=1" : string.Empty);

        var deals = await GetJsonAsync<List<DealDto>>(url, ct) ?? [];

        return deals
            .Where(d => !string.IsNullOrWhiteSpace(d.Title) && !string.IsNullOrWhiteSpace(d.DealID))
            .Select(d => new CheapSharkDeal(
                Title: d.Title!.Trim(),
                DealId: d.DealID!,
                StoreName: storeMap.GetValueOrDefault(d.StoreID ?? "", "Loja desconhecida"),
                SalePrice: ParseDecimal(d.SalePrice),
                NormalPrice: ParseDecimal(d.NormalPrice),
                SavingsPercent: ParseDecimal(d.Savings),
                Thumb: d.Thumb,
                RatingText: string.IsNullOrWhiteSpace(d.SteamRatingText) ? null : d.SteamRatingText))
            .ToList();
    }

    private async Task<IReadOnlyDictionary<string, string>> GetStoreMapAsync(CancellationToken ct)
    {
        var stores = await GetJsonAsync<List<StoreDto>>($"{_baseUrl}/stores", ct) ?? [];
        return stores
            .Where(s => !string.IsNullOrWhiteSpace(s.StoreID) && !string.IsNullOrWhiteSpace(s.StoreName))
            .ToDictionary(s => s.StoreID!, s => s.StoreName!);
    }

    private async Task<T?> GetJsonAsync<T>(string url, CancellationToken ct)
    {
        using var response = await http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
    }

    // A CheapShark serializa números como strings ("16.54", "58.000000").
    private static decimal ParseDecimal(string? s) =>
        decimal.TryParse(s, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0m;

    private sealed record DealDto(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("dealID")] string? DealID,
        [property: JsonPropertyName("storeID")] string? StoreID,
        [property: JsonPropertyName("salePrice")] string? SalePrice,
        [property: JsonPropertyName("normalPrice")] string? NormalPrice,
        [property: JsonPropertyName("savings")] string? Savings,
        [property: JsonPropertyName("thumb")] string? Thumb,
        [property: JsonPropertyName("steamRatingText")] string? SteamRatingText);

    private sealed record StoreDto(
        [property: JsonPropertyName("storeID")] string? StoreID,
        [property: JsonPropertyName("storeName")] string? StoreName);
}
