using Archive.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Archive.API.Controllers;

[ApiController]
[Route("api/favorite-brands")]
[Authorize]
[Produces("application/json")]
public class FavoriteBrandsController(IFavoriteBrandsService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAsync(UserId));

    [HttpPost("{brand}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(string brand)
    {
        if (!await service.AddAsync(UserId, brand))
            return Conflict(new { error = "Marca já está nos favoritos." });

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpDelete("{brand}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(string brand)
    {
        if (!await service.RemoveAsync(UserId, brand))
            return NotFound();

        return NoContent();
    }
}