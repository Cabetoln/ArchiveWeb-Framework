using Archive.API.Core.Contracts;
using Archive.API.DTOs;
using Archive.API.Models;
using Archive.API.Repositories;

namespace Archive.API.Services;

public class CatalogService(IItemRepository items, IProductSchema schema) : ICatalogService
{
    public async Task<PagedResult<ProductResponse>> SearchAsync(SearchProductsRequest req)
    {
        var query = (await items.GetAllAsync()).AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Query))
        {
            var term = req.Query.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Attributes.Values.Any(v => v != null && v.ToLower().Contains(term)));
        }

        if (req.MinPrice.HasValue)
            query = query.Where(p => p.CurrentPrice >= req.MinPrice.Value);

        if (req.MaxPrice.HasValue)
            query = query.Where(p => p.CurrentPrice <= req.MaxPrice.Value);

        query = schema.ApplyFilters(query, req);

        var total = query.Count();
        var page = Math.Max(1, req.Page);
        var pageSize = Math.Clamp(req.PageSize, 1, 100);

        var result = query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToResponse)
            .ToList();

        return new PagedResult<ProductResponse>(result, total, page, pageSize);
    }

    public Task<Product?> GetByIdAsync(Guid id) => items.GetByIdAsync(id);

    public async Task<Product> CreateAsync(CreateProductRequest req)
    {
        var product = schema.Build(req);

        var initialPrice = new PriceHistory
        {
            ProductId = product.Id,
            Price     = product.CurrentPrice,
            Currency  = product.Currency,
            Source    = "manual"
        };

        await items.AddAsync(product, initialPrice);
        return product;
    }

    public async Task<List<PriceHistoryResponse>?> GetPriceHistoryAsync(Guid productId, DateTime? from, DateTime? to)
    {
        if (await items.GetByIdAsync(productId) is null)
            return null;

        var query = (await items.GetPriceHistoryAsync(productId)).AsQueryable();

        if (from.HasValue)
            query = query.Where(p => p.RecordedAt >= from.Value.ToUniversalTime());

        if (to.HasValue)
            query = query.Where(p => p.RecordedAt <= to.Value.ToUniversalTime());

        return query
            .OrderBy(p => p.RecordedAt)
            .Select(p => new PriceHistoryResponse(p.Id, p.Price, p.Currency, p.RecordedAt, p.Source))
            .ToList();
    }

    public async Task<PriceHistoryResponse?> AddPriceAsync(Guid productId, AddPriceRequest req)
    {
        var entry = await items.RecordPriceAsync(productId, req.Price, req.Currency, req.Source);
        if (entry is null) return null;

        return new PriceHistoryResponse(entry.Id, entry.Price, entry.Currency, entry.RecordedAt, entry.Source);
    }

    private static ProductResponse ToResponse(Product p) =>
        new(p.Id, p.Name, p.ImageUrl, p.ProductUrl, p.CurrentPrice, p.Currency, p.UpdatedAt, p.Attributes);
}
