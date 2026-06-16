using System.Security.Claims;
using HealthSync.Core.DTOs.MedicalRecord;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/medical-records")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;

    public MedicalRecordsController(IMedicalRecordService medicalRecordService)
    {
        _medicalRecordService = medicalRecordService;
    }

    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetByPatient(Guid patientId)
    {
        var records = await _medicalRecordService.GetByPatientIdAsync(patientId);
        return Ok(records);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var record = await _medicalRecordService.GetByIdAsync(id);
        if (record == null) return NotFound();
        return Ok(record);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Create([FromBody] CreateMedicalRecordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _medicalRecordService.CreateAsync(dto, userId!);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicalRecordDto dto)
    {
        var result = await _medicalRecordService.UpdateAsync(id, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:guid}/prescriptions")]
    public async Task<IActionResult> GetPrescriptions(Guid id)
    {
        var prescriptions = await _medicalRecordService.GetPrescriptionsAsync(id);
        return Ok(prescriptions);
    }

    [HttpPost("{id:guid}/prescriptions")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> AddPrescription(Guid id, [FromBody] CreatePrescriptionDto dto)
    {
        var result = await _medicalRecordService.AddPrescriptionAsync(id, dto);
        return Ok(result);
    }
}
