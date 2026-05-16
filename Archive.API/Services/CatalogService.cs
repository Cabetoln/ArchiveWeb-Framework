using Archive.API.DTOs;
using Archive.API.Models;
using Archive.API.Repositories;

namespace Archive.API.Services;

public class CatalogService(IItemRepository items) : ICatalogService
{
    public async Task<PagedResult<FashionItemResponse>> SearchAsync(SearchItemsRequest req)
    {
        var query = (await items.GetAllAsync()).AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Query))
        {
            var term = req.Query.ToLower();
            query = query.Where(f =>
                f.Name.ToLower().Contains(term) ||
                f.Brand.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(req.Brand))
            query = query.Where(f => f.Brand.ToLower() == req.Brand.ToLower());

        if (!string.IsNullOrWhiteSpace(req.Category))
            query = query.Where(f => f.Category != null && f.Category.ToLower() == req.Category.ToLower());

        if (req.MinPrice.HasValue)
            query = query.Where(f => f.CurrentPrice >= req.MinPrice.Value);

        if (req.MaxPrice.HasValue)
            query = query.Where(f => f.CurrentPrice <= req.MaxPrice.Value);

        var total = query.Count();
        var page = Math.Max(1, req.Page);
        var pageSize = Math.Clamp(req.PageSize, 1, 100);

        var result = query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToResponse)
            .ToList();

        return new PagedResult<FashionItemResponse>(result, total, page, pageSize);
    }

    public Task<FashionItem?> GetByIdAsync(Guid id) => items.GetByIdAsync(id);

    public async Task<FashionItem> CreateAsync(CreateFashionItemRequest req)
    {
        var item = new FashionItem
        {
            Name = req.Name.Trim(),
            Brand = req.Brand.Trim(),
            Category = req.Category?.Trim(),
            ImageUrl = req.ImageUrl,
            ProductUrl = req.ProductUrl,
            CurrentPrice = req.CurrentPrice,
            Currency = req.Currency
        };

        var initialPrice = new PriceHistory
        {
            FashionItemId = item.Id,
            Price = item.CurrentPrice,
            Currency = item.Currency,
            Source = "manual"
        };

        await items.AddAsync(item, initialPrice);
        return item;
    }

    public async Task<List<PriceHistoryResponse>?> GetPriceHistoryAsync(Guid itemId, DateTime? from, DateTime? to)
    {
        if (await items.GetByIdAsync(itemId) is null)
            return null;

        var query = (await items.GetPriceHistoryAsync(itemId)).AsQueryable();

        if (from.HasValue)
            query = query.Where(p => p.RecordedAt >= from.Value.ToUniversalTime());

        if (to.HasValue)
            query = query.Where(p => p.RecordedAt <= to.Value.ToUniversalTime());

        return query
            .OrderBy(p => p.RecordedAt)
            .Select(p => new PriceHistoryResponse(p.Id, p.Price, p.Currency, p.RecordedAt, p.Source))
            .ToList();
    }

    public async Task<PriceHistoryResponse?> AddPriceAsync(Guid itemId, AddPriceRequest req)
    {
        var entry = await items.RecordPriceAsync(itemId, req.Price, req.Currency, req.Source);
        if (entry is null) return null;

        return new PriceHistoryResponse(entry.Id, entry.Price, entry.Currency, entry.RecordedAt, entry.Source);
    }

    private static FashionItemResponse ToResponse(FashionItem f) =>
        new(f.Id, f.Name, f.Brand, f.Category, f.ImageUrl, f.ProductUrl,
            f.CurrentPrice, f.Currency, f.UpdatedAt);
}
