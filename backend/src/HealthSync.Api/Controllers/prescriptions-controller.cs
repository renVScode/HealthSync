using System.Security.Claims;
using System.Text.Json;
using HealthSync.Core.DTOs.MedicalRecord;
using HealthSync.Core.Enums;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/prescriptions")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;
    private readonly IAuditService _auditService;

    public PrescriptionsController(IMedicalRecordService medicalRecordService, IAuditService auditService)
    {
        _medicalRecordService = medicalRecordService;
        _auditService = auditService;
    }

    [HttpGet("pharmacy")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> GetPharmacyQueue([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var prescriptions = await _medicalRecordService.GetPrescriptionsByStatusAsync(PrescriptionStatus.Paid, page, pageSize);
        return Ok(new { items = prescriptions, totalCount = prescriptions.Count, page, pageSize });
    }

    [HttpPost("{id:guid}/dispense")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> Dispense(Guid id, [FromBody] DispensePrescriptionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _medicalRecordService.DispensePrescriptionAsync(id, dto, userId);
        if (!result) return BadRequest(new { message = "Unable to dispense prescription. Check stock or prescription status." });

        await _auditService.LogAsync("dispense", "prescription", id, null,
            JsonSerializer.Serialize(dto),
            userId, HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(new { message = "Prescription dispensed successfully" });
    }
}
