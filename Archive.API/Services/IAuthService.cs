using Archive.API.Models;

namespace Archive.API.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(string name, string email, string password);
    Task<User?> AuthenticateAsync(string email, string password);
    Task<User?> GetByIdAsync(Guid id);
}
