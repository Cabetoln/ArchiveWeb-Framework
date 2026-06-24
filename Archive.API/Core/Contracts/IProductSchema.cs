using Archive.API.DTOs;
using Archive.API.Models;

namespace Archive.API.Core.Contracts;

public interface IProductSchema
{
    /// <summary>Nome do domínio exibido na UI ("Fashion", "Electronics", "Books"...).</summary>
    string DomainName { get; }

    /// <summary>Campos extras além do núcleo — define o que filtrar, exibir e validar.</summary>
    IReadOnlyList<AttributeDefinition> Attributes { get; }

    /// <summary>
    /// Campo usado para "grupos favoritos". Fashion usa "brand", Books usa "author".
    /// Null se o domínio não tiver esse conceito.
    /// </summary>
    IFavoritableGrouping? FavoritableGrouping { get; }

    /// <summary>Constrói um Product a partir de um request genérico de criação.</summary>
    Product Build(CreateProductRequest request);

    /// <summary>
    /// Aplica filtros de atributo específicos do domínio.
    /// O framework já filtra por texto e faixa de preço antes de chamar este método.
    /// </summary>
    IQueryable<Product> ApplyFilters(IQueryable<Product> query, SearchProductsRequest request);
}
