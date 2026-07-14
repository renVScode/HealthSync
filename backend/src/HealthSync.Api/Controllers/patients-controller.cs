using System.Security.Claims;
using System.Text.Json;
using HealthSync.Core.DTOs.Patient;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IAuditService _auditService;

    public PatientsController(IPatientService patientService, IAuditService auditService)
    {
        _patientService = patientService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] bool? isArchived = null)
    {
        var result = await _patientService.GetAllAsync(page, pageSize, search, isArchived);
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize });
    }

    [HttpGet("by-doctor/{doctorId:guid}")]
    public async Task<IActionResult> GetByDoctor(Guid doctorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? search = null)
    {
        var result = await _patientService.GetByDoctorIdAsync(doctorId, page, pageSize, search);
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Create([FromBody] CreatePatientDto dto)
    {
        var result = await _patientService.CreateAsync(dto);

        await _auditService.LogAsync("create", "patient", result.Id, null,
            JsonSerializer.Serialize(result),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientDto dto)
    {
        var oldPatient = await _patientService.GetByIdAsync(id);
        var result = await _patientService.UpdateAsync(id, dto);
        if (result == null) return NotFound();

        await _auditService.LogAsync("update", "patient", id,
            oldPatient != null ? JsonSerializer.Serialize(oldPatient) : null,
            JsonSerializer.Serialize(result),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/archive")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var oldPatient = await _patientService.GetByIdAsync(id);
        var result = await _patientService.ArchiveAsync(id);
        if (!result) return NotFound();

        await _auditService.LogAsync("archive", "patient", id,
            oldPatient != null ? JsonSerializer.Serialize(oldPatient) : null, null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return NoContent();
    }

    [HttpPatch("{id:guid}/restore")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _patientService.RestoreAsync(id);
        if (!result) return NotFound();

        var patient = await _patientService.GetByIdAsync(id);
        await _auditService.LogAsync("restore", "patient", id, null,
            patient != null ? JsonSerializer.Serialize(patient) : null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(patient);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var oldPatient = await _patientService.GetByIdAsync(id);
        var result = await _patientService.DeleteAsync(id);
        if (!result) return NotFound();

        await _auditService.LogAsync("delete", "patient", id,
            oldPatient != null ? JsonSerializer.Serialize(oldPatient) : null, null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var results = await _patientService.SearchAsync(q);
        return Ok(results);
    }
}
