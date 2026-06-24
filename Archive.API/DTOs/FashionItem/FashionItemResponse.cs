namespace Archive.API.DTOs;

public record ProductResponse(
    Guid Id,
    string Name,
    string? ImageUrl,
    string? ProductUrl,
    decimal CurrentPrice,
    string Currency,
    DateTime UpdatedAt,
    Dictionary<string, string?> Attributes
);
