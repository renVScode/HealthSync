using System.Security.Claims;
using System.Text.Json;
using HealthSync.Core.DTOs.Billing;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Receptionist")]
public class BillingsController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly IWebHostEnvironment _env;
    private readonly IAuditService _auditService;

    public BillingsController(IBillingService billingService, IWebHostEnvironment env, IAuditService auditService)
    {
        _billingService = billingService;
        _env = env;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] BillingFilterDto filter)
    {
        var result = await _billingService.GetAllAsync(filter);
        return Ok(new { result.Items, result.TotalCount, filter.Page, filter.PageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var billing = await _billingService.GetByIdAsync(id);
        if (billing == null) return NotFound();
        return Ok(billing);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBillingDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _billingService.CreateAsync(dto, userId);

        await _auditService.LogAsync("create", "billing", result.Id, null,
            JsonSerializer.Serialize(result),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("from-visit/{appointmentId:guid}")]
    public async Task<IActionResult> GenerateInvoiceFromVisit(Guid appointmentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var billing = await _billingService.GenerateInvoiceFromVisitAsync(appointmentId, userId);
        if (billing == null) return BadRequest(new { message = "Appointment not found or no medical record exists" });

        await _auditService.LogAsync("generate-invoice", "billing", billing.Id, null,
            JsonSerializer.Serialize(billing),
            userId, HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return CreatedAtAction(nameof(GetById), new { id = billing.Id }, billing);
    }

    [HttpPost("{id:guid}/payments")]
    public async Task<IActionResult> AddPayment(Guid id, [FromBody] CreatePaymentDto dto)
    {
        var oldBilling = await _billingService.GetByIdAsync(id);
        var result = await _billingService.AddPaymentAsync(id, dto);
        if (!result) return BadRequest(new { message = "Invoice already fully paid or cancelled" });

        await _auditService.LogAsync("add-payment", "billing", id,
            oldBilling != null ? JsonSerializer.Serialize(oldBilling) : null,
            JsonSerializer.Serialize(dto),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(new { message = "Payment recorded" });
    }

    [HttpPost("payments/{paymentId:guid}/verify")]
    public async Task<IActionResult> VerifyPayment(Guid paymentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _billingService.VerifyPaymentAsync(paymentId, userId);
        if (!result) return BadRequest(new { message = "Payment already verified or billing cancelled" });

        await _auditService.LogAsync("verify-payment", "payment", paymentId, null,
            JsonSerializer.Serialize(new { verifiedById = userId }),
            userId, HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(new { message = "Payment verified" });
    }

    [HttpPost("upload-qr")]
    public async Task<IActionResult> UploadQrCode(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "payments");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"qr_{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"/uploads/payments/{fileName}";

        await _auditService.LogAsync("upload-qr", "billing", null, null,
            JsonSerializer.Serialize(new { url }),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(new { url });
    }

    [HttpPost("{id:guid}/payments/{paymentId:guid}/upload-qr")]
    public async Task<IActionResult> UploadPaymentQrCode(Guid id, Guid paymentId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "payments");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{paymentId}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var imageUrl = $"/uploads/payments/{fileName}";
        await _billingService.UploadPaymentQrCodeAsync(paymentId, imageUrl);

        await _auditService.LogAsync("upload-payment-qr", "billing", id, null,
            JsonSerializer.Serialize(new { paymentId, imageUrl }),
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(new { url = imageUrl });
    }

    [HttpGet("{id:guid}/payments")]
    public async Task<IActionResult> GetPayments(Guid id)
    {
        var payments = await _billingService.GetPaymentsAsync(id);
        return Ok(payments);
    }
}
