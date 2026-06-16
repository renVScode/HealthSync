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

    public UsersController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
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
        var result = await _authService.UpdateUserAsync(id, dto);
        if (!result) return NotFound();
        return Ok(new { message = "User updated" });
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ToggleActivation(Guid id, [FromBody] ToggleActivationDto dto)
    {
        var result = await _authService.ToggleActivationAsync(id, dto.IsActive);
        if (!result) return NotFound();
        return Ok(new { message = dto.IsActive ? "User activated" : "User deactivated" });
    }

    [HttpGet("roles")]
    public IActionResult GetRoles()
    {
        var roles = Enum.GetNames<Core.Enums.UserRole>();
        return Ok(roles);
    }
}
