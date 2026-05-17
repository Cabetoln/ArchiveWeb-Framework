using Archive.API.Repositories;

namespace Archive.API.Services;

public class FavoriteBrandsService(IUserRepository users) : IFavoriteBrandsService
{
    public async Task<List<string>> GetAsync(Guid userId)
    {
        var user = await users.GetByIdAsync(userId);
        return user?.FavoriteBrands ?? [];
    }

    public async Task<bool> AddAsync(Guid userId, string brand)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) return false;

        var normalized = brand.Trim();
        if (user.FavoriteBrands.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            return false;

        user.FavoriteBrands.Add(normalized);
        await users.UpdateAsync(user);
        return true;
    }

    public async Task<bool> RemoveAsync(Guid userId, string brand)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) return false;

        var match = user.FavoriteBrands
            .FirstOrDefault(b => string.Equals(b, brand, StringComparison.OrdinalIgnoreCase));

        if (match is null) return false;

        user.FavoriteBrands.Remove(match);
        await users.UpdateAsync(user);
        return true;
    }
}
