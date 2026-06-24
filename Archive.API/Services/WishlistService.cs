using Archive.API.DTOs;
using Archive.API.Exceptions;
using Archive.API.Models;
using Archive.API.Repositories;

namespace Archive.API.Services;

public class WishlistService(IUserRepository users, IItemRepository items) : IWishlistService
{
    public async Task<List<WishlistEntryResponse>> GetWishlistAsync(Guid userId)
    {
        var allItems = await items.GetAllAsync();
        var user = await users.GetByIdAsync(userId);

        return (user?.WishlistEntries ?? [])
            .OrderByDescending(w => w.AddedAt)
            .Join(
                allItems,
                w => w.ProductId,
                i => i.Id,
                (w, i) => new WishlistEntryResponse(
                    w.Id, w.ProductId, i.Name,
                    i.Attributes.GetValueOrDefault("brand") ?? string.Empty,
                    i.CurrentPrice, i.ImageUrl, w.AddedAt, w.Note))
            .ToList();
    }

    public async Task<WishlistEntryResponse> AddAsync(Guid userId, Guid productId, string? note)
    {
        var item = await items.GetByIdAsync(productId);
        if (item is null)
            throw new BusinessException("Item não encontrado.", StatusCodes.Status404NotFound);

        var user = await users.GetByIdAsync(userId);
        if (user is null)
            throw new BusinessException("Usuário não encontrado.", StatusCodes.Status404NotFound);

        if (user.WishlistEntries.Any(w => w.ProductId == productId))
            throw new BusinessException("Item já está na wishlist.", StatusCodes.Status409Conflict);

        var entry = new WishlistEntry { UserId = userId, ProductId = productId, Note = note };
        user.WishlistEntries.Add(entry);
        await users.UpdateAsync(user);

        return new WishlistEntryResponse(
            entry.Id, item.Id, item.Name,
            item.Attributes.GetValueOrDefault("brand") ?? string.Empty,
            item.CurrentPrice, item.ImageUrl, entry.AddedAt, entry.Note);
    }

    public async Task<bool> RemoveAsync(Guid userId, Guid entryId)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) return false;

        var entry = user.WishlistEntries.FirstOrDefault(w => w.Id == entryId);
        if (entry is null) return false;

        user.WishlistEntries.Remove(entry);
        await users.UpdateAsync(user);
        return true;
    }
}
