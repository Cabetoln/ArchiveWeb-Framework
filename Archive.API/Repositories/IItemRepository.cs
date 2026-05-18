using Archive.API.Models;

namespace Archive.API.Repositories;

public interface IItemRepository
{
    Task<List<FashionItem>> GetAllAsync();
    Task<FashionItem?> GetByIdAsync(Guid id);
    Task<List<PriceHistory>> GetPriceHistoryAsync(Guid itemId);
    Task AddAsync(FashionItem item, PriceHistory initialPrice);
    Task<PriceHistory?> RecordPriceAsync(Guid itemId, decimal price, string currency, string? source);
    Task SetSeasonalAnalysisPendingAsync(bool pending);
    Task<bool> GetSeasonalAnalysisPendingAsync();
}
