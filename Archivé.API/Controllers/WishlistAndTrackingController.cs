using Archive.API.DTOs;
using Archive.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Archive.API.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize]
[Produces("application/json")]
public class WishlistController(IWishlistService wishlistService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Lista todos os itens da wishlist do usuário.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WishlistEntryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var entries = await wishlistService.GetWishlistAsync(UserId);
        return Ok(entries);
    }

    /// <summary>Adiciona um item à wishlist.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(WishlistEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] AddToWishlistRequest req)
    {
        var entry = await wishlistService.AddAsync(UserId, req.FashionItemId, req.Note);
        return CreatedAtAction(nameof(GetAll), entry);
    }

    /// <summary>Remove um item da wishlist.</summary>
    [HttpDelete("{entryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid entryId)
    {
        if (!await wishlistService.RemoveAsync(UserId, entryId)) return NotFound();
        return NoContent();
    }
}
