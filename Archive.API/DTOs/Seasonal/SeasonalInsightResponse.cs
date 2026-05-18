namespace Archive.API.DTOs;

public record SeasonalInsightResponse(
    Guid ItemId,
    bool HasEnoughHistory,
    bool HasSimulatedHistory,
    decimal CurrentPrice,
    decimal CurrentMonthAverage,
    decimal DifferencePercentage,
    string Status,
    string Recommendation,
    BestDiscountMonthResponse? BestDiscountMonth,
    IReadOnlyList<SeasonalPatternResponse> MonthlyPatterns
);
