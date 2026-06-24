namespace Archive.API.DTOs;

public record SearchProductsRequest
{
    public string? Query { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    // filtros de atributo do domínio — populados pelo controller a partir da query string
    public Dictionary<string, string> Attributes { get; init; } = [];
}
