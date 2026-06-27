using System.Security.Claims;
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

    public PrescriptionsController(IMedicalRecordService medicalRecordService)
    {
        _medicalRecordService = medicalRecordService;
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
        return Ok(new { message = "Prescription dispensed successfully" });
    }
}
