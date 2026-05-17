namespace Archive.API.DTOs;

public record PriceAlertResponse(
    Guid Id,
    Guid FashionItemId,
    string ItemName,
    string Brand,
    decimal TargetPrice,
    decimal CurrentPrice,
    string Currency,
    DateTime CreatedAt
);
