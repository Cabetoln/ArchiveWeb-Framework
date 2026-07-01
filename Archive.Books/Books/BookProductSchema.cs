using Archive.API.Core.Contracts;
using Archive.API.DTOs;
using Archive.API.Models;

namespace Archive.Books;

/// <summary>
/// Realização do ponto flexível <see cref="IProductSchema"/> para o domínio de livros.
/// Define os atributos próprios (autor, gênero, formato) e usa o autor como
/// agrupamento favoritável — reaproveitando todo o núcleo sem alterá-lo.
/// </summary>
public class BookProductSchema : IProductSchema
{
    public string DomainName => "Books";

    public IReadOnlyList<AttributeDefinition> Attributes =>
    [
        new("author", "Autor",   AttributeType.Text, IsFilterable: true, IsRequired: true),
        new("genre",  "Gênero",  AttributeType.Enum, IsFilterable: true, IsRequired: false,
            AllowedValues: ["Ficção", "Não-ficção", "Fantasia", "Técnico", "Poesia"]),
        new("format", "Formato", AttributeType.Enum, IsFilterable: true, IsRequired: false,
            AllowedValues: ["Capa dura", "Brochura", "E-book", "Audiolivro"]),
    ];

    public IFavoritableGrouping? FavoritableGrouping { get; } = new BookAuthorGrouping();

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
        foreach (var key in new[] { "author", "genre", "format" })
        {
            if (req.Attributes.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                query = query.Where(p =>
                    p.Attributes.ContainsKey(key) &&
                    p.Attributes[key] != null &&
                    p.Attributes[key]!.Equals(value, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }
}

file sealed class BookAuthorGrouping : IFavoritableGrouping
{
    public string Key => "author";
    public string DisplayName => "Autor";
}
