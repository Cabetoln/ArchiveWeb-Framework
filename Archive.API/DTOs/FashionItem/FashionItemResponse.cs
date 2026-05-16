namespace Archive.API.DTOs;

public record FashionItemResponse(
    Guid Id,
    string Name,
    string Brand,
    string? Category,
    string? ImageUrl,
    string? ProductUrl,
    decimal CurrentPrice,
    string Currency,
    DateTime UpdatedAt
);