using Archive.API.Models;
using System.Text.Json;

namespace Archive.API.Repositories;

public class JsonItemRepository : IItemRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonItemRepository(IHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "DataStore");
        Directory.CreateDirectory(dataDir);
        _filePath = Path.Combine(dataDir, "catalog.json");
    }

    public async Task<List<FashionItem>> GetAllAsync() =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            return data.FashionItems;
        });

    public async Task<FashionItem?> GetByIdAsync(Guid id) =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            return data.FashionItems.FirstOrDefault(x => x.Id == id);
        });

    public async Task<List<PriceHistory>> GetPriceHistoryAsync(Guid itemId) =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            return data.PriceHistories.Where(x => x.FashionItemId == itemId).ToList();
        });

    public async Task AddAsync(FashionItem item, PriceHistory initialPrice) =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            data.FashionItems.Add(item);
            data.PriceHistories.Add(initialPrice);
            await SaveDataAsync(data);
        });

    public async Task<PriceHistory?> RecordPriceAsync(Guid itemId, decimal price, string currency, string? source) =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            var item = data.FashionItems.FirstOrDefault(x => x.Id == itemId);
            if (item is null) return null;

            var entry = new PriceHistory
            {
                FashionItemId = itemId,
                Price = price,
                Currency = currency,
                Source = source
            };

            data.PriceHistories.Add(entry);
            item.CurrentPrice = price;
            item.Currency = currency;
            item.UpdatedAt = DateTime.UtcNow;

            await SaveDataAsync(data);
            return entry;
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

    private async Task<CatalogData> ReadDataAsync()
    {
        if (!File.Exists(_filePath)) return new CatalogData();
        var json = await File.ReadAllTextAsync(_filePath);
        if (string.IsNullOrWhiteSpace(json)) return new CatalogData();
        return JsonSerializer.Deserialize<CatalogData>(json) ?? new CatalogData();
    }

    private async Task SaveDataAsync(CatalogData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }

    private class CatalogData
    {
        public List<FashionItem> FashionItems { get; set; } = [];
        public List<PriceHistory> PriceHistories { get; set; } = [];
    }
}
