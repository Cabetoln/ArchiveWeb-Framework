using Archive.API.DTOs;
using Archive.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Archive.API.Controllers;

[ApiController]
[Route("api/price-alerts")]
[Authorize]
[Produces("application/json")]
public class PriceAlertsController(IPriceAlertService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Lista todos os alertas de preço do usuário com o preço atual de cada item.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PriceAlertResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAsync(UserId));

    /// <summary>Define ou atualiza um alerta de preço para um item.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PriceAlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Set([FromBody] SetPriceAlertRequest req)
    {
        var alert = await service.SetAsync(UserId, req.ProductId, req.TargetPrice);
        return Ok(alert);
    }

    /// <summary>Remove um alerta de preço.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid id)
    {
        if (!await service.RemoveAsync(UserId, id)) return NotFound();
        return NoContent();
    }
}
