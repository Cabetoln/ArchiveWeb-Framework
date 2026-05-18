namespace Archive.API.DTOs;

public record SeasonalPatternResponse(
    int Month,
    string MonthName,
    string Season,
    decimal? AveragePrice,
    decimal? DiscountPercentage,
    bool IsDiscountPeriod,
    bool HasData
);
