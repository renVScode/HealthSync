using System.Security.Claims;
using System.Text.Json;
using HealthSync.Core.DTOs.Medicine;
using HealthSync.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IAuditService _auditService;

    public InventoryController(IInventoryService inventoryService, IAuditService auditService)
    {
        _inventoryService = inventoryService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? search = null, [FromQuery] Guid? medicineId = null, [FromQuery] bool? expiringSoon = null)
    {
        if (page > 1 || !string.IsNullOrEmpty(search))
        {
            var result = await _inventoryService.GetPaginatedAsync(page, pageSize, search, medicineId, expiringSoon);
            return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize });
        }
        var batches = await _inventoryService.GetAllAsync(medicineId, expiringSoon);
        return Ok(batches);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var batch = await _inventoryService.GetByIdAsync(id);
        if (batch == null) return NotFound();
        return Ok(batch);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> AddBatch([FromBody] CreateInventoryBatchDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _inventoryService.AddBatchAsync(dto, userId!);

        await _auditService.LogAsync("create", "inventory-batch", result.Id, null,
            JsonSerializer.Serialize(result),
            userId, HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/dispense")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> Dispense(Guid id, [FromBody] DispenseMedicineDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _inventoryService.DispenseAsync(id, dto, userId!);
        if (!result) return BadRequest(new { message = "Insufficient stock" });

        await _auditService.LogAsync("dispense", "inventory-batch", id, null,
            JsonSerializer.Serialize(dto),
            userId, HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(new { message = "Dispensed successfully" });
    }

    [HttpPatch("{id:guid}/archive")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var oldBatch = await _inventoryService.GetByIdAsync(id);
        var result = await _inventoryService.ArchiveBatchAsync(id);
        if (!result) return NotFound();

        await _auditService.LogAsync("archive", "inventory-batch", id,
            oldBatch != null ? JsonSerializer.Serialize(oldBatch) : null, null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return NoContent();
    }

    [HttpPatch("{id:guid}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _inventoryService.RestoreBatchAsync(id);
        if (!result) return NotFound();

        var batch = await _inventoryService.GetByIdAsync(id);
        await _auditService.LogAsync("restore", "inventory-batch", id, null,
            batch != null ? JsonSerializer.Serialize(batch) : null,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"]);

        return Ok(batch);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var items = await _inventoryService.GetLowStockItemsAsync();
        return Ok(items);
    }

    [HttpGet("transactions")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> GetTransactions([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var utcFrom = from.HasValue ? (DateTime?)DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : null;
        var utcTo = to.HasValue ? (DateTime?)DateTime.SpecifyKind(to.Value, DateTimeKind.Utc) : null;
        var transactions = await _inventoryService.GetTransactionsAsync(utcFrom, utcTo);
        return Ok(transactions);
    }
}
