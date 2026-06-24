namespace Archive.API.DTOs;

public record CreateProductRequest(
    string Name,
    decimal CurrentPrice,
    string? ImageUrl = null,
    string? ProductUrl = null,
    string Currency = "BRL",
    Dictionary<string, string?>? Attributes = null
);
