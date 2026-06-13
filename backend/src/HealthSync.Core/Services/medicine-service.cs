using Microsoft.EntityFrameworkCore;
using HealthSync.Core.DTOs.Medicine;
using HealthSync.Core.Entities;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Core.Services;

public class MedicineService : IMedicineService
{
    private readonly IUnitOfWork _uow;

    public MedicineService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<MedicineResponseDto>> GetAllAsync(string? search, string? category)
    {
        var query = _uow.Medicines.Query().Include(m => m.InventoryBatches).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(term) || (m.GenericName != null && m.GenericName.ToLower().Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(m => m.Category == category);

        var medicines = await query.Where(m => m.IsActive).ToListAsync();
        return medicines.Select(m => new MedicineResponseDto
        {
            Id = m.Id,
            Name = m.Name,
            GenericName = m.GenericName,
            Category = m.Category,
            Manufacturer = m.Manufacturer,
            Unit = m.Unit,
            Price = m.Price,
            ReorderLevel = m.ReorderLevel,
            Description = m.Description,
            IsActive = m.IsActive,
            CurrentStock = m.InventoryBatches.Sum(b => b.Quantity)
        });
    }

    public async Task<MedicineResponseDto?> GetByIdAsync(Guid id)
    {
        var medicine = await _uow.Medicines.Query()
            .Include(m => m.InventoryBatches)
            .FirstOrDefaultAsync(m => m.Id == id);

        return medicine == null ? null : new MedicineResponseDto
        {
            Id = medicine.Id,
            Name = medicine.Name,
            GenericName = medicine.GenericName,
            Category = medicine.Category,
            Manufacturer = medicine.Manufacturer,
            Unit = medicine.Unit,
            Price = medicine.Price,
            ReorderLevel = medicine.ReorderLevel,
            Description = medicine.Description,
            IsActive = medicine.IsActive,
            CurrentStock = medicine.InventoryBatches.Sum(b => b.Quantity)
        };
    }

    public async Task<MedicineResponseDto> CreateAsync(CreateMedicineDto dto)
    {
        var medicine = new Medicine
        {
            Name = dto.Name,
            GenericName = dto.GenericName,
            Category = dto.Category,
            Manufacturer = dto.Manufacturer,
            Unit = dto.Unit,
            Price = dto.Price,
            ReorderLevel = dto.ReorderLevel,
            Description = dto.Description
        };
        await _uow.Medicines.AddAsync(medicine);
        await _uow.SaveChangesAsync();
        return (await GetByIdAsync(medicine.Id))!;
    }

    public async Task<MedicineResponseDto?> UpdateAsync(Guid id, UpdateMedicineDto dto)
    {
        var medicine = await _uow.Medicines.GetByIdAsync(id);
        if (medicine == null) return null;
        if (dto.Name != null) medicine.Name = dto.Name;
        if (dto.GenericName != null) medicine.GenericName = dto.GenericName;
        if (dto.Category != null) medicine.Category = dto.Category;
        if (dto.Manufacturer != null) medicine.Manufacturer = dto.Manufacturer;
        if (dto.Unit != null) medicine.Unit = dto.Unit;
        if (dto.Price.HasValue) medicine.Price = dto.Price.Value;
        if (dto.ReorderLevel.HasValue) medicine.ReorderLevel = dto.ReorderLevel.Value;
        if (dto.Description != null) medicine.Description = dto.Description;
        medicine.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task DeactivateAsync(Guid id)
    {
        var medicine = await _uow.Medicines.GetByIdAsync(id);
        if (medicine != null)
        {
            medicine.IsActive = false;
            await _uow.SaveChangesAsync();
        }
    }
}
