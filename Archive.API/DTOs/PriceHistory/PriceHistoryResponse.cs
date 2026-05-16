namespace Archive.API.DTOs;
public record PriceHistoryResponse(
    Guid Id,
    decimal Price,
    string Currency,
    DateTime RecordedAt,
    string? Source
);