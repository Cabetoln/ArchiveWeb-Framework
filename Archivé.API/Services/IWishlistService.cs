using Archive.API.DTOs;

namespace Archive.API.Services;

public interface IWishlistService
{
    Task<List<WishlistEntryResponse>> GetWishlistAsync(Guid userId);
    Task<WishlistEntryResponse> AddAsync(Guid userId, Guid fashionItemId, string? note);
    Task<bool> RemoveAsync(Guid userId, Guid entryId);
}
