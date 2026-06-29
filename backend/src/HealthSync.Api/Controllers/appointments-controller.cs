using System.Security.Claims;
using System.Text.Json;
using HealthSync.Core.DTOs.Appointment;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IAuditService _auditService;

    public AppointmentsController(IAppointmentService appointmentService, IAuditService auditService)
    {
        _appointmentService = appointmentService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AppointmentFilterDto filter)
    {
        var result = await _appointmentService.GetAllAsync(filter);
        return Ok(new { result.Items, result.TotalCount, filter.Page, filter.PageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null) return NotFound();
        return Ok(appointment);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _appointmentService.CreateAsync(dto, userId!);

        await _auditService.LogAsync("create", "appointment", result.Id, null,
            JsonSerializer.Serialize(result),
            userId, HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentDto dto)
    {
        var oldAppointment = await _appointmentService.GetByIdAsync(id);
        var result = await _appointmentService.UpdateAsync(id, dto);
        if (result == null) return NotFound();

        await _auditService.LogAsync("update", "appointment", id,
            oldAppointment != null ? JsonSerializer.Serialize(oldAppointment) : null,
            JsonSerializer.Serialize(result),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var oldAppointment = await _appointmentService.GetByIdAsync(id);
        var success = await _appointmentService.UpdateStatusAsync(id, dto.Status, userId!);
        if (!success) return BadRequest(new { message = "Invalid status transition" });

        await _auditService.LogAsync("update-status", "appointment", id,
            oldAppointment != null ? JsonSerializer.Serialize(oldAppointment) : null,
            JsonSerializer.Serialize(new { Status = dto.Status.ToString(), updatedById = userId }),
            userId, HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return NoContent();
    }

    [HttpPatch("{id:guid}/archive")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var oldAppointment = await _appointmentService.GetByIdAsync(id);
        var result = await _appointmentService.ArchiveAsync(id);
        if (!result) return NotFound();

        await _auditService.LogAsync("archive", "appointment", id,
            oldAppointment != null ? JsonSerializer.Serialize(oldAppointment) : null, null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return NoContent();
    }

    [HttpPatch("{id:guid}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _appointmentService.RestoreAsync(id);
        if (!result) return NotFound();

        var appointment = await _appointmentService.GetByIdAsync(id);
        await _auditService.LogAsync("restore", "appointment", id, null,
            appointment != null ? JsonSerializer.Serialize(appointment) : null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(appointment);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var oldAppointment = await _appointmentService.GetByIdAsync(id);
        var result = await _appointmentService.CancelAsync(id);
        if (!result) return NotFound();

        await _auditService.LogAsync("delete", "appointment", id,
            oldAppointment != null ? JsonSerializer.Serialize(oldAppointment) : null, null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return NoContent();
    }

    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendarEvents([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] Guid? doctorId)
    {
        var events = await _appointmentService.GetCalendarEventsAsync(start, end, doctorId);
        return Ok(events);
    }
}
