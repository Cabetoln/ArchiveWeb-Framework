using Archive.API.Exceptions;
using Archive.API.Models;
using Archive.API.Repositories;

namespace Archive.API.Services;

public class AuthService(IUserRepository users) : IAuthService
{
    public async Task<User> RegisterAsync(string name, string email, string password)
    {
        var normalized = email.Trim().ToLowerInvariant();
        if (await users.GetByEmailAsync(normalized) is not null)
            throw new BusinessException("E-mail já cadastrado.", StatusCodes.Status409Conflict);

        var user = new User
        {
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        await users.AddAsync(user);
        return user;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await users.GetByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;
        return user;
    }

    public Task<User?> GetByIdAsync(Guid id) => users.GetByIdAsync(id);
}
