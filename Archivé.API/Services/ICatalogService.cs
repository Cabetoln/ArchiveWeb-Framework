using Archive.API.DTOs;
using Archive.API.Models;

namespace Archive.API.Services;

public interface ICatalogService
{
    Task<PagedResult<FashionItemResponse>> SearchAsync(SearchItemsRequest req);
    Task<FashionItem?> GetByIdAsync(Guid id);
    Task<FashionItem> CreateAsync(CreateFashionItemRequest req);
    Task<List<PriceHistoryResponse>?> GetPriceHistoryAsync(Guid itemId, DateTime? from, DateTime? to);
    Task<PriceHistoryResponse?> AddPriceAsync(Guid itemId, AddPriceRequest req);
}
