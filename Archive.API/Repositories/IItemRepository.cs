using Archive.API.Models;

namespace Archive.API.Repositories;

public interface IItemRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task<List<PriceHistory>> GetPriceHistoryAsync(Guid productId);
    Task AddAsync(Product product, PriceHistory initialPrice);
    Task<PriceHistory?> RecordPriceAsync(Guid productId, decimal price, string currency, string? source);
    Task SetSeasonalAnalysisPendingAsync(bool pending);
    Task<bool> GetSeasonalAnalysisPendingAsync();
}
