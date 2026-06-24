using Archive.API.DTOs;
using Archive.API.Models;

namespace Archive.API.Services;

public interface ICatalogService
{
    Task<PagedResult<ProductResponse>> SearchAsync(SearchProductsRequest req);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(CreateProductRequest req);
    Task<List<PriceHistoryResponse>?> GetPriceHistoryAsync(Guid productId, DateTime? from, DateTime? to);
    Task<PriceHistoryResponse?> AddPriceAsync(Guid productId, AddPriceRequest req);

    /// <summary>
    /// Sincroniza o catálogo a partir do scraper de preços do domínio ativo
    /// (ponto flexível <c>IPriceScraper</c>): adiciona itens novos e registra
    /// alterações de preço dos já existentes (casados por <c>ProductUrl</c>).
    /// </summary>
    Task<ScrapeSyncResult> SyncFromScraperAsync(CancellationToken ct = default);
}
