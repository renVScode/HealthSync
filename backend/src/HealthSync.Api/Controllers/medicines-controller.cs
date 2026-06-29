using System.Security.Claims;
using System.Text.Json;
using HealthSync.Core.DTOs.Medicine;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicinesController : ControllerBase
{
    private readonly IMedicineService _medicineService;
    private readonly IAuditService _auditService;

    public MedicinesController(IMedicineService medicineService, IAuditService auditService)
    {
        _medicineService = medicineService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? category)
    {
        var medicines = await _medicineService.GetAllAsync(search, category);
        return Ok(medicines);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var medicine = await _medicineService.GetByIdAsync(id);
        if (medicine == null) return NotFound();
        return Ok(medicine);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> Create([FromBody] CreateMedicineDto dto)
    {
        var result = await _medicineService.CreateAsync(dto);

        await _auditService.LogAsync("create", "medicine", result.Id, null,
            JsonSerializer.Serialize(result),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicineDto dto)
    {
        var oldMedicine = await _medicineService.GetByIdAsync(id);
        var result = await _medicineService.UpdateAsync(id, dto);
        if (result == null) return NotFound();

        await _auditService.LogAsync("update", "medicine", id,
            oldMedicine != null ? JsonSerializer.Serialize(oldMedicine) : null,
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
        var oldMedicine = await _medicineService.GetByIdAsync(id);
        await _medicineService.DeactivateAsync(id);

        await _auditService.LogAsync("deactivate", "medicine", id,
            oldMedicine != null ? JsonSerializer.Serialize(oldMedicine) : null, null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return NoContent();
    }
}
