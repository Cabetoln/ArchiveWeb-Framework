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
                w => w.FashionItemId,
                i => i.Id,
                (w, i) => new WishlistEntryResponse(
                    w.Id, w.FashionItemId, i.Name, i.Brand,
                    i.CurrentPrice, i.ImageUrl, w.AddedAt, w.Note))
            .ToList();
    }

    public async Task<WishlistEntryResponse> AddAsync(Guid userId, Guid fashionItemId, string? note)
    {
        var item = await items.GetByIdAsync(fashionItemId);
        if (item is null)
            throw new BusinessException("Item não encontrado.", StatusCodes.Status404NotFound);

        var user = await users.GetByIdAsync(userId);
        if (user is null)
            throw new BusinessException("Usuário não encontrado.", StatusCodes.Status404NotFound);

        if (user.WishlistEntries.Any(w => w.FashionItemId == fashionItemId))
            throw new BusinessException("Item já está na wishlist.", StatusCodes.Status409Conflict);

        var entry = new WishlistEntry
        {
            UserId = userId,
            FashionItemId = fashionItemId,
            Note = note
        };

        user.WishlistEntries.Add(entry);
        await users.UpdateAsync(user);

        return new WishlistEntryResponse(
            entry.Id, item.Id, item.Name, item.Brand,
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
