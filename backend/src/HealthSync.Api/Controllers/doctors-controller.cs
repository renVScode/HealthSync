using HealthSync.Core.DTOs.Doctor;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _doctorService.GetAllAsync();
        return Ok(doctors);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null) return NotFound();
        return Ok(doctor);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto)
    {
        var result = await _doctorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorDto dto)
    {
        var result = await _doctorService.UpdateAsync(id, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:guid}/availability")]
    public async Task<IActionResult> GetAvailability(Guid id)
    {
        var availability = await _doctorService.GetAvailabilityAsync(id);
        return Ok(availability);
    }

    [HttpPut("{id:guid}/availability")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> UpdateAvailability(Guid id, [FromBody] List<UpsertAvailabilityDto> dtos)
    {
        await _doctorService.UpdateAvailabilityAsync(id, dtos);
        return NoContent();
    }

    [HttpGet("{id:guid}/time-offs")]
    public async Task<IActionResult> GetTimeOffs(Guid id)
    {
        var timeOffs = await _doctorService.GetTimeOffsAsync(id);
        return Ok(timeOffs);
    }

    [HttpPost("{id:guid}/time-offs")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> RequestTimeOff(Guid id, [FromBody] CreateTimeOffDto dto)
    {
        var result = await _doctorService.RequestTimeOffAsync(id, dto);
        return Ok(result);
    }

    [HttpGet("{id:guid}/slots")]
    public async Task<IActionResult> GetAvailableSlots(Guid id, [FromQuery] DateTime date)
    {
        var slots = await _doctorService.GetAvailableSlotsAsync(id, date);
        return Ok(slots);
    }
}
