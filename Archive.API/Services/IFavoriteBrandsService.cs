namespace Archive.API.Services;

public interface IFavoriteBrandsService
{
    Task<List<string>> GetAsync(Guid userId);
    Task<bool> AddAsync(Guid userId, string brand);
    Task<bool> RemoveAsync(Guid userId, string brand);
}
