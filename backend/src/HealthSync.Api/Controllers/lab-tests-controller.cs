using System.Security.Claims;
using System.Text.Json;
using HealthSync.Core.DTOs.Lab;
using HealthSync.Core.Enums;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LabTestsController : ControllerBase
{
    private readonly ILabService _labService;
    private readonly IAuditService _auditService;

    public LabTestsController(ILabService labService, IAuditService auditService)
    {
        _labService = labService;
        _auditService = auditService;
    }

    // --- Catalog ---

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? search = null)
    {
        if (page > 1 || !string.IsNullOrEmpty(search))
        {
            var result = await _labService.GetLabTestsAsync(page, pageSize, search);
            return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize });
        }
        var tests = await _labService.GetAllLabTestsAsync();
        return Ok(tests);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var test = await _labService.GetLabTestByIdAsync(id);
        if (test == null) return NotFound();
        return Ok(test);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateLabTestDto dto)
    {
        var result = await _labService.CreateLabTestAsync(dto);

        await _auditService.LogAsync("create", "lab-test", result.Id, null,
            JsonSerializer.Serialize(result),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLabTestDto dto)
    {
        var old = await _labService.GetLabTestByIdAsync(id);
        var result = await _labService.UpdateLabTestAsync(id, dto);
        if (result == null) return NotFound();

        await _auditService.LogAsync("update", "lab-test", id,
            old != null ? JsonSerializer.Serialize(old) : null,
            JsonSerializer.Serialize(result),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _labService.DeleteLabTestAsync(id);
        if (!deleted) return NotFound();

        await _auditService.LogAsync("delete", "lab-test", id, null, null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return NoContent();
    }

    // --- Orders ---

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25,
        [FromQuery] LabOrderStatus? status = null,
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] string? search = null)
    {
        var result = await _labService.GetLabOrdersAsync(page, pageSize, status, patientId, doctorId, search);
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize });
    }

    [HttpGet("orders/{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _labService.GetLabOrderByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPost("orders")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateLabOrderDto dto,
        [FromServices] HealthSync.Core.Interfaces.Services.IDoctorService doctorService)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Guid doctorId;

        if (User.IsInRole("Admin") && dto.DoctorId.HasValue)
        {
            doctorId = dto.DoctorId.Value;
        }
        else
        {
            var doctor = await doctorService.GetByUserIdAsync(userId);
            if (doctor == null)
                return BadRequest(new { message = "Doctor profile not found" });
            doctorId = doctor.Id;
        }

        var result = await _labService.CreateLabOrderAsync(doctorId, dto);

        await _auditService.LogAsync("create-lab-order", "lab-order", result.Id, null,
            JsonSerializer.Serialize(result),
            userId,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(result);
    }

    [HttpPut("orders/{id:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] UpdateLabOrderDto dto)
    {
        var result = await _labService.UpdateLabOrderAsync(id, dto);
        if (result == null) return NotFound();

        await _auditService.LogAsync("update-lab-order", "lab-order", id, null,
            JsonSerializer.Serialize(result),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(result);
    }

    [HttpDelete("orders/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        var deleted = await _labService.DeleteLabOrderAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
