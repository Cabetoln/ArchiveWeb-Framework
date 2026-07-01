using Archive.API.Core.Contracts;
using Archive.API.DTOs;
using Archive.API.Exceptions;
using Archive.API.Models;
using Archive.API.Repositories;

namespace Archive.API.Services;

public class PriceAlertService(IUserRepository users, IItemRepository items, IProductSchema schema) : IPriceAlertService
{
    private string GroupingValue(Product item) =>
        schema.FavoritableGrouping?.ExtractValue(item) ?? string.Empty;

    public async Task<List<PriceAlertResponse>> GetAsync(Guid userId)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) return [];

        var allItems = await items.GetAllAsync();

        return user.PriceAlerts
            .Join(allItems,
                a => a.ProductId,
                i => i.Id,
                (a, i) => new PriceAlertResponse(
                    a.Id, i.Id, i.Name,
                    GroupingValue(i),
                    a.TargetPrice, i.CurrentPrice, i.Currency, a.CreatedAt))
            .ToList();
    }

    public async Task<PriceAlertResponse> SetAsync(Guid userId, Guid productId, decimal targetPrice)
    {
        var item = await items.GetByIdAsync(productId);
        if (item is null)
            throw new BusinessException("Item não encontrado.", StatusCodes.Status404NotFound);

        var user = await users.GetByIdAsync(userId);
        if (user is null)
            throw new BusinessException("Usuário não encontrado.", StatusCodes.Status404NotFound);

        var existing = user.PriceAlerts.FirstOrDefault(a => a.ProductId == productId);
        if (existing is not null)
        {
            existing.TargetPrice = targetPrice;
        }
        else
        {
            existing = new PriceAlert { UserId = userId, ProductId = productId, TargetPrice = targetPrice };
            user.PriceAlerts.Add(existing);
        }

        await users.UpdateAsync(user);

        return new PriceAlertResponse(
            existing.Id, item.Id, item.Name,
            GroupingValue(item),
            existing.TargetPrice, item.CurrentPrice, item.Currency, existing.CreatedAt);
    }

    public async Task<bool> RemoveAsync(Guid userId, Guid alertId)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) return false;

        var alert = user.PriceAlerts.FirstOrDefault(a => a.Id == alertId);
        if (alert is null) return false;

        user.PriceAlerts.Remove(alert);
        await users.UpdateAsync(user);
        return true;
    }
}
