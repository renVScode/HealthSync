using System.Security.Claims;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IDoctorService _doctorService;

    public ReportsController(IReportService reportService, IDoctorService doctorService)
    {
        _reportService = reportService;
        _doctorService = doctorService;
    }

    private static DateTime ToUtc(DateTime dt) => DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    private async Task<Guid?> GetCurrentDoctorIdAsync()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Doctor") return null;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var doctor = await _doctorService.GetByUserIdAsync(userId!);
        return doctor?.Id;
    }

    [HttpGet("appointment-summary")]
    public async Task<IActionResult> GetAppointmentSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var doctorId = await GetCurrentDoctorIdAsync();
        var summary = await _reportService.GetAppointmentSummaryAsync(from.HasValue ? ToUtc(from.Value) : DateTime.UtcNow.AddMonths(-1), to.HasValue ? ToUtc(to.Value) : DateTime.UtcNow, doctorId);
        return Ok(summary);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var doctorId = await GetCurrentDoctorIdAsync();
        var revenue = await _reportService.GetRevenueAsync(from.HasValue ? ToUtc(from.Value) : DateTime.UtcNow.AddMonths(-1), to.HasValue ? ToUtc(to.Value) : DateTime.UtcNow, doctorId);
        return Ok(revenue);
    }

    [HttpGet("doctor-performance")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDoctorPerformance([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var performance = await _reportService.GetDoctorPerformanceAsync(from.HasValue ? ToUtc(from.Value) : DateTime.UtcNow.AddMonths(-1), to.HasValue ? ToUtc(to.Value) : DateTime.UtcNow);
        return Ok(performance);
    }

    [HttpGet("inventory-summary")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> GetInventorySummary()
    {
        var summary = await _reportService.GetInventorySummaryAsync();
        return Ok(summary);
    }

    [HttpGet("patient-visits")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetPatientVisits([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var doctorId = await GetCurrentDoctorIdAsync();
        var visits = await _reportService.GetPatientVisitsAsync(from.HasValue ? ToUtc(from.Value) : DateTime.UtcNow.AddMonths(-3), to.HasValue ? ToUtc(to.Value) : DateTime.UtcNow, doctorId);
        return Ok(visits);
    }
}
