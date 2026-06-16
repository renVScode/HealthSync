using System.Security.Claims;
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

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
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
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _appointmentService.CreateAsync(dto, userId!);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentDto dto)
    {
        var result = await _appointmentService.UpdateAsync(id, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _appointmentService.UpdateStatusAsync(id, dto.Status, userId!);
        if (!result) return BadRequest(new { message = "Invalid status transition" });
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _appointmentService.CancelAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendarEvents([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] Guid? doctorId)
    {
        var events = await _appointmentService.GetCalendarEventsAsync(start, end, doctorId);
        return Ok(events);
    }
}
