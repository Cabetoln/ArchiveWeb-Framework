namespace Archive.API.DTOs;

public record CreateFashionItemRequest(
    string Name,
    string Brand,
    string? Category,
    string? ImageUrl,
    string? ProductUrl,
    decimal CurrentPrice,
    string Currency = "BRL"
);