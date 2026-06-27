using System.Security.Claims;
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

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
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
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/dispense")]
    [Authorize(Roles = "Admin,Pharmacist")]
    public async Task<IActionResult> Dispense(Guid id, [FromBody] DispenseMedicineDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _inventoryService.DispenseAsync(id, dto, userId!);
        if (!result) return BadRequest(new { message = "Insufficient stock" });
        return Ok(new { message = "Dispensed successfully" });
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
        var transactions = await _inventoryService.GetTransactionsAsync(from, to);
        return Ok(transactions);
    }
}
