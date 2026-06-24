namespace Archive.API.Core.Contracts;

public interface IPriceScraper
{
    string Name { get; }
    Task<IReadOnlyList<ScrapedProduct>> ScrapeAsync(CancellationToken ct = default);
}

public record ScrapedProduct(
    string Name,
    string? ProductUrl,
    string? ImageUrl,
    decimal Price,
    string Currency,
    Dictionary<string, string?> Attributes
);
