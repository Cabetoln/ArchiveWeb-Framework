using Archive.API.Models;
using System.Text.Json;

namespace Archive.API.Repositories;

public class JsonUserRepository : IUserRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonUserRepository(IHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "DataStore");
        Directory.CreateDirectory(dataDir);
        _filePath = Path.Combine(dataDir, "users.json");
    }

    public async Task<User?> GetByEmailAsync(string email) =>
        await WithLockAsync(async () =>
        {
            var users = await ReadAllAsync();
            return users.FirstOrDefault(u => u.Email == email);
        });

    public async Task<User?> GetByIdAsync(Guid id) =>
        await WithLockAsync(async () =>
        {
            var users = await ReadAllAsync();
            return users.FirstOrDefault(u => u.Id == id);
        });

    public async Task AddAsync(User user) =>
        await WithLockAsync(async () =>
        {
            var users = await ReadAllAsync();
            users.Add(user);
            await SaveAllAsync(users);
        });

    public async Task UpdateAsync(User user) =>
        await WithLockAsync(async () =>
        {
            var users = await ReadAllAsync();
            var index = users.FindIndex(u => u.Id == user.Id);
            if (index >= 0) users[index] = user;
            await SaveAllAsync(users);
        });

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<T> WithLockAsync<T>(Func<Task<T>> action)
    {
        await _gate.WaitAsync();
        try { return await action(); }
        finally { _gate.Release(); }
    }

    private async Task WithLockAsync(Func<Task> action)
    {
        await _gate.WaitAsync();
        try { await action(); }
        finally { _gate.Release(); }
    }

    private async Task<List<User>> ReadAllAsync()
    {
        if (!File.Exists(_filePath)) return [];
        var json = await File.ReadAllTextAsync(_filePath);
        if (string.IsNullOrWhiteSpace(json)) return [];
        return JsonSerializer.Deserialize<List<User>>(json) ?? [];
    }

    private async Task SaveAllAsync(List<User> users)
    {
        var json = JsonSerializer.Serialize(users, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
