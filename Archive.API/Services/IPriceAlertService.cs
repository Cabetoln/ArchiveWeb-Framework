using Archive.API.DTOs;

namespace Archive.API.Services;

public interface IPriceAlertService
{
    Task<List<PriceAlertResponse>> GetAsync(Guid userId);
    Task<PriceAlertResponse> SetAsync(Guid userId, Guid fashionItemId, decimal targetPrice);
    Task<bool> RemoveAsync(Guid userId, Guid alertId);
}
