namespace Archive.API.DTOs;

public record BestDiscountMonthResponse(
    int Month,
    string MonthName,
    string Season,
    decimal DiscountPercentage,
    bool IsDiscountPeriod,
    string Insight
);
