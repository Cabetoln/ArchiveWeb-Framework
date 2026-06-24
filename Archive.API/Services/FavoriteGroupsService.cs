using Archive.API.Core.Contracts;
using Archive.API.Models;
using Archive.API.Repositories;

namespace Archive.API.Services;

/// <summary>
/// Serviço de "grupos favoritos" dirigido pelo ponto flexível
/// <see cref="IFavoritableGrouping"/>. A chave do grupo (ex: "brand" no Fashion,
/// "author" em Books) vem do schema de domínio ativo, e não de um campo fixo.
/// </summary>
public class FavoriteGroupsService(IUserRepository users, IProductSchema schema) : IFavoriteGroupsService
{
    private string GroupingKey =>
        schema.FavoritableGrouping?.Key
        ?? throw new InvalidOperationException(
            "O domínio ativo não define um agrupamento favoritável (IFavoritableGrouping).");

    public async Task<List<string>> GetAsync(Guid userId)
    {
        var user = await users.GetByIdAsync(userId);
        return Group(user) ?? [];
    }

    public async Task<bool> AddAsync(Guid userId, string value)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) return false;

        var group = GetOrCreateGroup(user);
        var normalized = value.Trim();
        if (group.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            return false;

        group.Add(normalized);
        await users.UpdateAsync(user);
        return true;
    }

    public async Task<bool> RemoveAsync(Guid userId, string value)
    {
        var user = await users.GetByIdAsync(userId);
        if (user is null) return false;

        var group = Group(user);
        var match = group?.FirstOrDefault(b => string.Equals(b, value, StringComparison.OrdinalIgnoreCase));
        if (match is null) return false;

        group!.Remove(match);
        await users.UpdateAsync(user);
        return true;
    }

    private List<string>? Group(User? user) =>
        user is not null && user.FavoriteGroups.TryGetValue(GroupingKey, out var group) ? group : null;

    private List<string> GetOrCreateGroup(User user)
    {
        if (!user.FavoriteGroups.TryGetValue(GroupingKey, out var group))
            user.FavoriteGroups[GroupingKey] = group = [];
        return group;
    }
}
