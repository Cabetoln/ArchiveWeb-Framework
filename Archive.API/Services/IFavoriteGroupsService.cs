namespace Archive.API.Services;

public interface IFavoriteGroupsService
{
    Task<List<string>> GetAsync(Guid userId);
    Task<bool> AddAsync(Guid userId, string value);
    Task<bool> RemoveAsync(Guid userId, string value);
}
