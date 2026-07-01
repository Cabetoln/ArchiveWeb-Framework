using Archive.API.Core.Contracts;
using Archive.API.DTOs;
using Archive.API.Models;

namespace Archive.Games;

/// <summary>
/// Realização do ponto flexível <see cref="IProductSchema"/> para o domínio de
/// ofertas de games. Define os atributos próprios (loja, avaliação, faixa de
/// desconto) e usa a <b>loja</b> como agrupamento favoritável — reaproveitando
/// todo o núcleo do framework sem alterá-lo.
///
/// As faixas de desconto (<c>AllowedValues</c> do atributo <c>discount</c>) vêm
/// da <see cref="IDiscountTierStrategy"/> injetada, de modo que a política de
/// faixas tenha uma única fonte de verdade compartilhada com o scraper.
/// </summary>
public class GameProductSchema(IDiscountTierStrategy discountTiers) : IProductSchema
{
    public string DomainName => "Games";

    public IReadOnlyList<AttributeDefinition> Attributes =>
    [
        new("store",    "Loja",      AttributeType.Text, IsFilterable: true, IsRequired: true),
        new("rating",   "Avaliação", AttributeType.Enum, IsFilterable: true, IsRequired: false,
            AllowedValues:
            [
                "Overwhelmingly Positive", "Very Positive", "Positive",
                "Mostly Positive", "Mixed", "Mostly Negative", "Sem avaliação"
            ]),
        new("discount", "Desconto",  AttributeType.Enum, IsFilterable: true, IsRequired: false,
            AllowedValues: [.. discountTiers.Tiers]),
    ];

    public IFavoritableGrouping? FavoritableGrouping { get; } = new GameStoreGrouping();

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
        foreach (var key in new[] { "store", "rating", "discount" })
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

/// <summary>
/// Agrupamento favoritável do domínio de games: a <b>loja</b> (Steam, GOG,
/// Humble Bundle...). Permite ao usuário acompanhar suas lojas preferidas.
/// </summary>
file sealed class GameStoreGrouping : IFavoritableGrouping
{
    public string Key => "store";
    public string DisplayName => "Loja";
}
