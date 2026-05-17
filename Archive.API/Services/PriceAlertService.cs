using Archive.API.DTOs;
using Archive.API.Exceptions;
using Archive.API.Models;
using Archive.API.Repositories;

namespace Archive.API.Services;

public class PriceAlertService(IUserRepository users, IItemRepository items) : IPriceAlertService
{
    public async Task<List<PriceAlertResponse>> GetAsync(Guid userId)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) return [];

        var allItems = await items.GetAllAsync();

        return user.PriceAlerts
            .Join(allItems,
                a => a.FashionItemId,
                i => i.Id,
                (a, i) => new PriceAlertResponse(
                    a.Id, i.Id, i.Name, i.Brand,
                    a.TargetPrice, i.CurrentPrice, i.Currency, a.CreatedAt))
            .ToList();
    }

    public async Task<PriceAlertResponse> SetAsync(Guid userId, Guid fashionItemId, decimal targetPrice)
    {
        var item = await items.GetByIdAsync(fashionItemId);
        if (item is null)
            throw new BusinessException("Item não encontrado.", StatusCodes.Status404NotFound);

        var user = await users.GetByIdAsync(userId);
        if (user is null)
            throw new BusinessException("Usuário não encontrado.", StatusCodes.Status404NotFound);

        // Se já existe alerta para esse item, atualiza o preço-alvo
        var existing = user.PriceAlerts.FirstOrDefault(a => a.FashionItemId == fashionItemId);
        if (existing is not null)
        {
            existing.TargetPrice = targetPrice;
        }
        else
        {
            existing = new PriceAlert
            {
                UserId = userId,
                FashionItemId = fashionItemId,
                TargetPrice = targetPrice
            };
            user.PriceAlerts.Add(existing);
        }

        await users.UpdateAsync(user);

        return new PriceAlertResponse(
            existing.Id, item.Id, item.Name, item.Brand,
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
