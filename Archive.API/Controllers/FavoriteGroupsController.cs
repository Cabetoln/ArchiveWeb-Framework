using Archive.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Archive.API.Controllers;

[ApiController]
[Route("api/favorite-groups")]
[Authorize]
[Produces("application/json")]
public class FavoriteGroupsController(IFavoriteGroupsService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAsync(UserId));

    [HttpPost("{value}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(string value)
    {
        if (!await service.AddAsync(UserId, value))
            return Conflict(new { error = "Já está nos favoritos." });

        return NoContent();
    }

    [HttpDelete("{value}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(string value)
    {
        if (!await service.RemoveAsync(UserId, value))
            return NotFound();

        return NoContent();
    }
}
