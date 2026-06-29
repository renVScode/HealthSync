using Microsoft.EntityFrameworkCore;
using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.Medicine;
using HealthSync.Core.Entities;
using HealthSync.Core.Enums;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Core.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _uow;

    public InventoryService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<InventoryBatchResponseDto>> GetAllAsync(Guid? medicineId, bool? expiringSoon)
    {
        var query = _uow.InventoryBatches.Query()
            .Include(b => b.Medicine)
            .AsQueryable();

        if (medicineId.HasValue)
            query = query.Where(b => b.MedicineId == medicineId);
        if (expiringSoon == true)
        {
            var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90));
            query = query.Where(b => b.ExpiryDate <= threshold && b.ExpiryDate > DateOnly.FromDateTime(DateTime.UtcNow));
        }

        var batches = await query.OrderBy(b => b.ExpiryDate).ToListAsync();
        return batches.Select(MapToDto);
    }

    public async Task<PaginatedResult<InventoryBatchResponseDto>> GetPaginatedAsync(int page, int pageSize, string? search, Guid? medicineId, bool? expiringSoon)
    {
        var query = _uow.InventoryBatches.Query()
            .Include(b => b.Medicine)
            .AsQueryable();

        if (medicineId.HasValue)
            query = query.Where(b => b.MedicineId == medicineId);
        if (expiringSoon == true)
        {
            var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90));
            query = query.Where(b => b.ExpiryDate <= threshold && b.ExpiryDate > DateOnly.FromDateTime(DateTime.UtcNow));
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(b => b.Medicine.Name.ToLower().Contains(term)
                                  || b.BatchNumber.ToLower().Contains(term)
                                  || (b.Supplier != null && b.Supplier.ToLower().Contains(term)));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(b => b.ExpiryDate)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PaginatedResult<InventoryBatchResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private static InventoryBatchResponseDto MapToDto(InventoryBatch b) => new()
    {
        Id = b.Id,
        MedicineId = b.MedicineId,
        MedicineName = b.Medicine.Name,
        BatchNumber = b.BatchNumber,
        Quantity = b.Quantity,
        UnitPrice = b.UnitPrice,
        ExpiryDate = b.ExpiryDate?.ToDateTime(TimeOnly.MinValue),
        Supplier = b.Supplier,
        IsLowStock = b.Quantity <= b.Medicine.ReorderLevel
    };

    public async Task<InventoryBatchResponseDto?> GetByIdAsync(Guid id)
    {
        var batch = await _uow.InventoryBatches.Query()
            .Include(b => b.Medicine)
            .FirstOrDefaultAsync(b => b.Id == id);

        return batch == null ? null : new InventoryBatchResponseDto
        {
            Id = batch.Id,
            MedicineId = batch.MedicineId,
            MedicineName = batch.Medicine.Name,
            BatchNumber = batch.BatchNumber,
            Quantity = batch.Quantity,
            UnitPrice = batch.UnitPrice,
            ExpiryDate = batch.ExpiryDate?.ToDateTime(TimeOnly.MinValue),
            Supplier = batch.Supplier,
            IsLowStock = batch.Quantity <= batch.Medicine.ReorderLevel
        };
    }

    public async Task<InventoryBatchResponseDto> AddBatchAsync(CreateInventoryBatchDto dto, string userId)
    {
        var batch = new InventoryBatch
        {
            MedicineId = dto.MedicineId,
            BatchNumber = dto.BatchNumber,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            ManufactureDate = dto.ManufactureDate.HasValue ? DateOnly.FromDateTime(dto.ManufactureDate.Value) : null,
            ExpiryDate = dto.ExpiryDate.HasValue ? DateOnly.FromDateTime(dto.ExpiryDate.Value) : null,
            Supplier = dto.Supplier,
            Location = dto.Location
        };

        await _uow.InventoryBatches.AddAsync(batch);

        var transaction = new InventoryTransaction
        {
            InventoryBatchId = batch.Id,
            TransactionType = TransactionType.StockIn,
            Quantity = dto.Quantity,
            ReferenceType = "Purchase",
            Notes = dto.Supplier != null ? $"From: {dto.Supplier}" : null,
            CreatedById = Guid.Parse(userId)
        };
        await _uow.InventoryTransactions.AddAsync(transaction);
        await _uow.SaveChangesAsync();

        return (await GetByIdAsync(batch.Id))!;
    }

    public async Task<bool> DispenseAsync(Guid batchId, DispenseMedicineDto dto, string userId)
    {
        var batch = await _uow.InventoryBatches.GetByIdAsync(batchId);
        if (batch == null || batch.Quantity < dto.Quantity) return false;

        batch.Quantity -= dto.Quantity;
        var transaction = new InventoryTransaction
        {
            InventoryBatchId = batchId,
            TransactionType = TransactionType.Dispensed,
            Quantity = dto.Quantity,
            ReferenceType = "Prescription",
            ReferenceId = dto.PrescriptionId,
            Notes = dto.Notes,
            CreatedById = Guid.Parse(userId)
        };

        await _uow.InventoryTransactions.AddAsync(transaction);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ArchiveBatchAsync(Guid id)
    {
        var batch = await _uow.InventoryBatches.GetByIdAsync(id);
        if (batch == null) return false;
        batch.IsArchived = true;
        batch.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreBatchAsync(Guid id)
    {
        var batch = await _uow.InventoryBatches.GetByIdAsync(id);
        if (batch == null) return false;
        batch.IsArchived = false;
        batch.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<LowStockItemDto>> GetLowStockItemsAsync()
    {
        var items = await _uow.Medicines.Query()
            .Where(m => m.IsActive)
            .Select(m => new
            {
                Medicine = m,
                CurrentStock = m.InventoryBatches.Sum(b => b.Quantity)
            })
            .Where(x => x.CurrentStock <= x.Medicine.ReorderLevel)
            .ToListAsync();

        return items.Select(x => new LowStockItemDto
        {
            MedicineId = x.Medicine.Id,
            MedicineName = x.Medicine.Name,
            CurrentStock = x.CurrentStock,
            ReorderLevel = x.Medicine.ReorderLevel
        });
    }

    public async Task<IEnumerable<InventoryTransaction>> GetTransactionsAsync(DateTime? from, DateTime? to)
    {
        var query = _uow.InventoryTransactions.Query().Include(t => t.InventoryBatch.Medicine).AsQueryable();
        if (from.HasValue) query = query.Where(t => t.CreatedAt >= from);
        if (to.HasValue) query = query.Where(t => t.CreatedAt <= to);
        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }
}
