using System.Security.Claims;
using System.Text.Json;
using HealthSync.Core.DTOs.Auth;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public UsersController(IAuthService authService, IAuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? search = null, [FromQuery] bool? isArchived = null)
    {
        if (page > 1 || !string.IsNullOrEmpty(search) || isArchived.HasValue)
        {
            var result = await _authService.GetAllUsersAsync(page, pageSize, search, isArchived);
            return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize });
        }
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _authService.GetUserByIdAsync(id.ToString());
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var oldUser = await _authService.GetUserByIdAsync(id.ToString());
        var result = await _authService.UpdateUserAsync(id, dto);
        if (!result) return NotFound();

        await _auditService.LogAsync("update", "user", id,
            oldUser != null ? JsonSerializer.Serialize(oldUser) : null,
            JsonSerializer.Serialize(dto),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(new { message = "User updated" });
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ToggleActivation(Guid id, [FromBody] ToggleActivationDto dto)
    {
        var oldUser = await _authService.GetUserByIdAsync(id.ToString());
        var result = await _authService.ToggleActivationAsync(id, dto.IsActive);
        if (!result) return NotFound();

        await _auditService.LogAsync("toggle-activation", "user", id,
            oldUser != null ? JsonSerializer.Serialize(oldUser) : null,
            JsonSerializer.Serialize(dto),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(new { message = dto.IsActive ? "User activated" : "User deactivated" });
    }

    [HttpPatch("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var oldUser = await _authService.GetUserByIdAsync(id.ToString());
        var result = await _authService.ArchiveAsync(id);
        if (!result) return NotFound();

        await _auditService.LogAsync("archive", "user", id,
            oldUser != null ? JsonSerializer.Serialize(oldUser) : null, null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return NoContent();
    }

    [HttpPatch("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _authService.RestoreAsync(id);
        if (!result) return NotFound();

        var user = await _authService.GetUserByIdAsync(id.ToString());
        await _auditService.LogAsync("restore", "user", id, null,
            user != null ? JsonSerializer.Serialize(user) : null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(user);
    }

    [HttpGet("roles")]
    public IActionResult GetRoles()
    {
        var roles = Enum.GetNames<Core.Enums.UserRole>();
        return Ok(roles);
    }
}
