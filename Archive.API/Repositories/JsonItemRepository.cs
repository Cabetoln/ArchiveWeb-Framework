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

    public async Task<List<Product>> GetAllAsync() =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            return data.Products;
        });

    public async Task<Product?> GetByIdAsync(Guid id) =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            return data.Products.FirstOrDefault(x => x.Id == id);
        });

    public async Task<List<PriceHistory>> GetPriceHistoryAsync(Guid productId) =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            return data.PriceHistories.Where(x => x.ProductId == productId).ToList();
        });

    public async Task AddAsync(Product product, PriceHistory initialPrice) =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            data.Products.Add(product);
            data.PriceHistories.Add(initialPrice);
            data.SeasonalAnalysisPending = true;
            await SaveDataAsync(data);
        });

    public async Task<PriceHistory?> RecordPriceAsync(Guid productId, decimal price, string currency, string? source) =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            var product = data.Products.FirstOrDefault(x => x.Id == productId);
            if (product is null) return null;

            var entry = new PriceHistory
            {
                ProductId = productId,
                Price = price,
                Currency = currency,
                Source = source
            };

            data.PriceHistories.Add(entry);
            product.CurrentPrice = price;
            product.Currency = currency;
            product.UpdatedAt = DateTime.UtcNow;
            data.SeasonalAnalysisPending = true;

            await SaveDataAsync(data);
            return entry;
        });

    public async Task SetSeasonalAnalysisPendingAsync(bool pending) =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            data.SeasonalAnalysisPending = pending;
            await SaveDataAsync(data);
        });

    public async Task<bool> GetSeasonalAnalysisPendingAsync() =>
        await WithLockAsync(async () =>
        {
            var data = await ReadDataAsync();
            return data.SeasonalAnalysisPending;
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
        public List<Product> Products { get; set; } = [];
        public List<PriceHistory> PriceHistories { get; set; } = [];
        public bool SeasonalAnalysisPending { get; set; } = false;
    }
}
