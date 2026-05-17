using Archive.API.DTOs;
using Archive.API.Models;
using Archive.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Archive.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Registra um novo usuário.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var user = await authService.RegisterAsync(req.Name, req.Email, req.Password);
        await SignInAsync(user);
        return CreatedAtAction(nameof(Me), null, ToResponse(user));
    }

    /// <summary>Autentica o usuário e inicia a sessão.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await authService.AuthenticateAsync(req.Email, req.Password);
        if (user is null) return Unauthorized(new { error = "Credenciais inválidas." });

        await SignInAsync(user);
        return Ok(ToResponse(user));
    }

    /// <summary>Encerra a sessão do usuário.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    /// <summary>Retorna o usuário autenticado.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await authService.GetByIdAsync(userId);
        return user is null ? Unauthorized() : Ok(ToResponse(user));
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task SignInAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) });
    }

    private static UserResponse ToResponse(User u) =>
        new(u.Id, u.Name, u.Email, u.CreatedAt);
}
