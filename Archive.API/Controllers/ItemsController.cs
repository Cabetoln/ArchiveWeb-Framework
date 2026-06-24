using Archive.API.Core.Contracts;
using Archive.API.DTOs;
using Archive.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.API.Controllers;

[ApiController]
[Route("api/items")]
[Produces("application/json")]
public class ItemsController(
    ICatalogService catalogService,
    ISeasonalAnalysisService seasonalAnalysisService,
    IImageSearchProvider imageSearch,
    IProductSchema schema) : ControllerBase
{
    private static readonly HashSet<string> CoreQueryKeys =
        new(StringComparer.OrdinalIgnoreCase) { "query", "minprice", "maxprice", "page", "pagesize" };

    /// <summary>Busca produtos com filtros genéricos. Parâmetros de atributo do domínio
    /// (ex: brand=Nike&amp;category=Tênis) são coletados automaticamente.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] SearchProductsRequest req)
    {
        var attrs = Request.Query
            .Where(kv => !CoreQueryKeys.Contains(kv.Key) && kv.Value.Count > 0)
            .ToDictionary(kv => kv.Key.ToLower(), kv => kv.Value.ToString());

        var result = await catalogService.SearchAsync(req with { Attributes = attrs });
        return Ok(result);
    }

    /// <summary>Retorna o schema do domínio ativo (campos disponíveis para filtro).</summary>
    [HttpGet("schema")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSchema() =>
        Ok(new
        {
            schema.DomainName,
            schema.Attributes,
            FavoritableGrouping = schema.FavoritableGrouping is null ? null : new
            {
                schema.FavoritableGrouping.Key,
                schema.FavoritableGrouping.DisplayName,
            }
        });

    /// <summary>Busca um produto específico pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await catalogService.GetByIdAsync(id);
        if (item is null) return NotFound();

        return Ok(new ProductResponse(
            item.Id, item.Name, item.ImageUrl, item.ProductUrl,
            item.CurrentPrice, item.Currency, item.UpdatedAt, item.Attributes));
    }

    /// <summary>Cadastra um novo produto.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest req)
    {
        var item = await catalogService.CreateAsync(req);

        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            new ProductResponse(
                item.Id, item.Name, item.ImageUrl, item.ProductUrl,
                item.CurrentPrice, item.Currency, item.UpdatedAt, item.Attributes));
    }

    /// <summary>Retorna o histórico de preços de um produto.</summary>
    [HttpGet("{id:guid}/price-history")]
    [ProducesResponseType(typeof(IEnumerable<PriceHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPriceHistory(
        Guid id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var history = await catalogService.GetPriceHistoryAsync(id, from, to);
        return history is null ? NotFound() : Ok(history);
    }

    [HttpGet("{id:guid}/seasonal-insight")]
    [ProducesResponseType(typeof(SeasonalInsightResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeasonalInsight(Guid id)
    {
        var insight = await seasonalAnalysisService.GetInsightAsync(id);
        return insight is null ? NotFound() : Ok(insight);
    }

    /// <summary>Sincroniza o catálogo a partir do scraper de preços do domínio ativo
    /// (ponto flexível <c>IPriceScraper</c>): adiciona itens novos e atualiza preços.</summary>
    [HttpPost("sync")]
    [Authorize]
    [ProducesResponseType(typeof(ScrapeSyncResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sync(CancellationToken ct) =>
        Ok(await catalogService.SyncFromScraperAsync(ct));

    /// <summary>Busca produtos por imagem usando o provedor de busca visual do domínio
    /// ativo (ponto flexível <c>IImageSearchProvider</c>).</summary>
    [HttpPost("search-by-image")]
    [ProducesResponseType(typeof(IEnumerable<ImageSearchMatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SearchByImage(IFormFile image, [FromQuery] int topK = 12, CancellationToken ct = default)
    {
        if (!imageSearch.IsAvailable)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "Busca por imagem indisponível no domínio ativo." });

        if (image is null || image.Length == 0)
            return BadRequest(new { error = "Imagem não enviada." });

        await using var stream = image.OpenReadStream();
        var matches = await imageSearch.SearchAsync(stream, topK, ct);

        var results = new List<ImageSearchMatchResponse>();
        foreach (var match in matches)
        {
            var product = await catalogService.GetByIdAsync(match.ProductId);
            if (product is null) continue;

            results.Add(new ImageSearchMatchResponse(
                new ProductResponse(
                    product.Id, product.Name, product.ImageUrl, product.ProductUrl,
                    product.CurrentPrice, product.Currency, product.UpdatedAt, product.Attributes),
                match.Score));
        }

        return Ok(results);
    }

    /// <summary>Adiciona um novo registro de preço para um produto.</summary>
    [HttpPost("{id:guid}/price-history")]
    [Authorize]
    [ProducesResponseType(typeof(PriceHistoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPrice(Guid id, [FromBody] AddPriceRequest req)
    {
        var entry = await catalogService.AddPriceAsync(id, req);
        if (entry is null) return NotFound();

        return CreatedAtAction(nameof(GetPriceHistory), new { id }, entry);
    }
}
