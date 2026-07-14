using System.Security.Claims;
using System.Text.Json;
using HealthSync.Core.DTOs.Billing;
using HealthSync.Core.DTOs.MedicalRecord;
using HealthSync.Core.Enums;
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
    private readonly IAuditService _auditService;
    private readonly IBillingService _billingService;

    public MedicalRecordsController(IMedicalRecordService medicalRecordService, IAuditService auditService, IBillingService billingService)
    {
        _medicalRecordService = medicalRecordService;
        _auditService = auditService;
        _billingService = billingService;
    }

    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetByPatient(Guid patientId)
    {
        var records = await _medicalRecordService.GetByPatientIdAsync(patientId);
        return Ok(records);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? isArchived = null, [FromQuery] string? status = null)
    {
        var result = await _medicalRecordService.GetAllAsync(page, pageSize, isArchived, status);
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize });
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

        await _auditService.LogAsync("create", "medical-record", result.Id, null,
            JsonSerializer.Serialize(result),
            userId, HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicalRecordDto dto)
    {
        var oldRecord = await _medicalRecordService.GetByIdAsync(id);
        var result = await _medicalRecordService.UpdateAsync(id, dto);
        if (result == null) return NotFound();

        await _auditService.LogAsync("update", "medical-record", id,
            oldRecord != null ? JsonSerializer.Serialize(oldRecord) : null,
            JsonSerializer.Serialize(result),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

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

        await _auditService.LogAsync("add-prescription", "medical-record", id, null,
            JsonSerializer.Serialize(result),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(result);
    }

    [HttpPost("{id:guid}/prescriptions/batch")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> AddPrescriptions(Guid id, [FromBody] List<CreatePrescriptionDto> dtos)
    {
        var result = await _medicalRecordService.AddPrescriptionsAsync(id, dtos);

        await _auditService.LogAsync("add-prescriptions", "medical-record", id, null,
            JsonSerializer.Serialize(result),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/complete")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var result = await _medicalRecordService.CompleteAsync(id);
        if (!result) return NotFound(new { message = "Record not found or already completed" });

        await _auditService.LogAsync("complete", "medical-record", id,
            null, null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return NoContent();
    }

    [HttpPatch("{id:guid}/archive")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var oldRecord = await _medicalRecordService.GetByIdAsync(id);
        var result = await _medicalRecordService.ArchiveAsync(id);
        if (!result) return NotFound();

        await _auditService.LogAsync("archive", "medical-record", id,
            oldRecord != null ? JsonSerializer.Serialize(oldRecord) : null, null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return NoContent();
    }

    [HttpPatch("{id:guid}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _medicalRecordService.RestoreAsync(id);
        if (!result) return NotFound();

        var record = await _medicalRecordService.GetByIdAsync(id);
        await _auditService.LogAsync("restore", "medical-record", id, null,
            record != null ? JsonSerializer.Serialize(record) : null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(record);
    }

    [HttpPost("{medicalRecordId:guid}/prescriptions/mark-paid")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> MarkPrescriptionsPaid(Guid medicalRecordId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _medicalRecordService.MarkPrescriptionsAsPaidAsync(medicalRecordId);
        if (!result) return NotFound(new { message = "No pending prescriptions found for this record" });

        var record = await _medicalRecordService.GetByIdAsync(medicalRecordId);
        if (record?.AppointmentId != null)
        {
            var existingBilling = await _billingService.GetByAppointmentIdAsync(record.AppointmentId.Value);
            if (existingBilling == null)
            {
                var billing = await _billingService.GenerateInvoiceFromVisitAsync(record.AppointmentId.Value, userId!);
                if (billing != null)
                {
                    await _billingService.AddPaymentAsync(billing.Id, new CreatePaymentDto
                    {
                        Amount = billing.Total,
                        PaymentMethod = PaymentMethod.Cash
                    }, Guid.Parse(userId!));
                }
            }
        }

        await _auditService.LogAsync("mark-paid", "prescription", medicalRecordId, null,
            JsonSerializer.Serialize(new { medicalRecordId }),
            userId, HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(new { message = "Prescriptions marked as paid" });
    }
}
