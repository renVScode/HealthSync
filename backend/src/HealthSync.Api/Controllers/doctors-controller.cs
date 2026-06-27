using System.Security.Claims;
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
    private readonly IWebHostEnvironment _env;

    public DoctorsController(IDoctorService doctorService, IWebHostEnvironment env)
    {
        _doctorService = doctorService;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? search = null)
    {
        if (page > 1 || !string.IsNullOrEmpty(search))
        {
            var result = await _doctorService.GetAllAsync(page, pageSize, search);
            return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize });
        }
        var doctors = await _doctorService.GetAllAsync();
        return Ok(doctors);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var doctor = await _doctorService.GetByUserIdAsync(userId);
        if (doctor == null) return NotFound();
        return Ok(doctor);
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

    [HttpPut("{id:guid}/profile")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateDoctorDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        var isOwner = doctor.UserId.ToString() == userId;
        if (!isAdmin && !isOwner)
            return Forbid();

        var result = await _doctorService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpPost("{id:guid}/upload-image")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, [FromQuery] string type = "profile")
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        var isOwner = doctor.UserId.ToString() == userId;
        if (!isAdmin && !isOwner)
            return Forbid();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "doctors");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{id}_{type}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var imageUrl = $"/uploads/doctors/{fileName}";
        var updateDto = new UpdateDoctorDto();
        if (type == "license")
            updateDto.LicenseImageUrl = imageUrl;
        else
            updateDto.ProfileImageUrl = imageUrl;

        await _doctorService.UpdateAsync(id, updateDto);
        return Ok(new { url = imageUrl });
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
