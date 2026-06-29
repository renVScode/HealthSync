using System.Security.Claims;
using System.Text.Json;
using HealthSync.Core.DTOs.Auth;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public AuthController(IAuthService authService, IAuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        var userInfo = result.User!;
        if (!result.Success)
            return Unauthorized(new { message = result.ErrorMessage });

        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            expiresAt = result.ExpiresAt,
            user = new { userInfo.Id, userInfo.FirstName, userInfo.LastName, userInfo.Email, userInfo.Role }
        });
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        var userId = Guid.Parse(result.UserId!);

        await _auditService.LogAsync("register", "user", userId, null,
            JsonSerializer.Serialize(request),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(new { message = "User created successfully", userId = result.UserId });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (!result.Success)
            return Unauthorized(new { message = result.ErrorMessage });

        return Ok(new { accessToken = result.AccessToken, refreshToken = result.RefreshToken, expiresAt = result.ExpiresAt });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _authService.RevokeRefreshTokenAsync(userId!);
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _authService.GetUserByIdAsync(userId!);
        if (user == null) return NotFound();
        return Ok(new { user.Id, user.FirstName, user.LastName, user.Email, user.Role });
    }
}
