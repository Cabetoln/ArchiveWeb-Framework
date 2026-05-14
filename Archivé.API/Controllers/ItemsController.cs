using Archive.API.DTOs;
using Archive.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.API.Controllers;

[ApiController]
[Route("api/items")]
[Produces("application/json")]
public class ItemsController(ICatalogService catalogService) : ControllerBase
{
    /// <summary>Busca itens de moda com filtros opcionais.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FashionItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] SearchItemsRequest req)
    {
        var result = await catalogService.SearchAsync(req);
        return Ok(result);
    }

    /// <summary>Busca um item específico pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FashionItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await catalogService.GetByIdAsync(id);
        if (item is null) return NotFound();

        return Ok(new FashionItemResponse(
            item.Id, item.Name, item.Brand, item.Category,
            item.ImageUrl, item.ProductUrl, item.CurrentPrice, item.Currency, item.UpdatedAt));
    }

    /// <summary>Cadastra um novo item de moda.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(FashionItemResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateFashionItemRequest req)
    {
        var item = await catalogService.CreateAsync(req);

        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            new FashionItemResponse(
                item.Id, item.Name, item.Brand, item.Category,
                item.ImageUrl, item.ProductUrl, item.CurrentPrice, item.Currency, item.UpdatedAt));
    }

    /// <summary>Retorna o histórico de preços de um item.</summary>
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

    /// <summary>Adiciona um novo registro de preço para um item.</summary>
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
