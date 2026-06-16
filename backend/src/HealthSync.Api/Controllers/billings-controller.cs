using HealthSync.Core.DTOs.Billing;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Receptionist")]
public class BillingsController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingsController(IBillingService billingService)
    {
        _billingService = billingService;
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
        var result = await _billingService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/payments")]
    public async Task<IActionResult> AddPayment(Guid id, [FromBody] CreatePaymentDto dto)
    {
        var result = await _billingService.AddPaymentAsync(id, dto);
        if (!result) return BadRequest(new { message = "Invoice already fully paid or cancelled" });
        return Ok(new { message = "Payment recorded" });
    }

    [HttpGet("{id:guid}/payments")]
    public async Task<IActionResult> GetPayments(Guid id)
    {
        var payments = await _billingService.GetPaymentsAsync(id);
        return Ok(payments);
    }
}
