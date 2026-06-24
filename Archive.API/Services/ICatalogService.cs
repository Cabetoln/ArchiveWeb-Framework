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
}
