namespace Archive.API.DTOs;

/// <summary>Resumo de uma sincronização de catálogo a partir de um <c>IPriceScraper</c>.</summary>
public record ScrapeSyncResult(
    string Source,
    int Scraped,
    int Added,
    int Updated,
    int Unchanged
);
