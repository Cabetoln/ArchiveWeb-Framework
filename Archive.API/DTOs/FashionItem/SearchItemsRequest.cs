namespace Archive.API.DTOs;

public record SearchItemsRequest(
    string? Query,
    string? Brand,
    string? Category,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20
);