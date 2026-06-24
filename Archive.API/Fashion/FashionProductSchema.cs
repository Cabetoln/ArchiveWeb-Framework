using Archive.API.Core.Contracts;
using Archive.API.DTOs;
using Archive.API.Models;

namespace Archive.API.Fashion;

public class FashionProductSchema : IProductSchema
{
    public string DomainName => "Fashion";

    public IReadOnlyList<AttributeDefinition> Attributes =>
    [
        new("brand",    "Marca",     AttributeType.Text, IsFilterable: true, IsRequired: true),
        new("category", "Categoria", AttributeType.Enum, IsFilterable: true, IsRequired: false,
            AllowedValues: ["Camisetas", "Jaquetas", "Calçados", "Calças"]),
    ];

    public IFavoritableGrouping? FavoritableGrouping { get; } = new FashionBrandGrouping();

    public Product Build(CreateProductRequest req) => new()
    {
        Name         = req.Name.Trim(),
        CurrentPrice = req.CurrentPrice,
        Currency     = req.Currency,
        ImageUrl     = req.ImageUrl,
        ProductUrl   = req.ProductUrl,
        Attributes   = req.Attributes ?? []
    };

    public IQueryable<Product> ApplyFilters(IQueryable<Product> query, SearchProductsRequest req)
    {
        if (req.Attributes.TryGetValue("brand", out var brand) && !string.IsNullOrWhiteSpace(brand))
            query = query.Where(p =>
                p.Attributes.ContainsKey("brand") &&
                p.Attributes["brand"] != null &&
                p.Attributes["brand"]!.Equals(brand, StringComparison.OrdinalIgnoreCase));

        if (req.Attributes.TryGetValue("category", out var category) && !string.IsNullOrWhiteSpace(category))
            query = query.Where(p =>
                p.Attributes.ContainsKey("category") &&
                p.Attributes["category"] != null &&
                p.Attributes["category"]!.Equals(category, StringComparison.OrdinalIgnoreCase));

        return query;
    }
}

file sealed class FashionBrandGrouping : IFavoritableGrouping
{
    public string Key => "brand";
    public string DisplayName => "Marca";
}
