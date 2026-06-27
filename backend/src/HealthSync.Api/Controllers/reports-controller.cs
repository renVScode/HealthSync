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

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    private static DateTime ToUtc(DateTime dt) => DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    [HttpGet("appointment-summary")]
    public async Task<IActionResult> GetAppointmentSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var summary = await _reportService.GetAppointmentSummaryAsync(from.HasValue ? ToUtc(from.Value) : DateTime.UtcNow.AddMonths(-1), to.HasValue ? ToUtc(to.Value) : DateTime.UtcNow);
        return Ok(summary);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var revenue = await _reportService.GetRevenueAsync(from.HasValue ? ToUtc(from.Value) : DateTime.UtcNow.AddMonths(-1), to.HasValue ? ToUtc(to.Value) : DateTime.UtcNow);
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
        var visits = await _reportService.GetPatientVisitsAsync(from.HasValue ? ToUtc(from.Value) : DateTime.UtcNow.AddMonths(-3), to.HasValue ? ToUtc(to.Value) : DateTime.UtcNow);
        return Ok(visits);
    }
}
