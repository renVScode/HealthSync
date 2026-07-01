using Microsoft.EntityFrameworkCore;
using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.Lab;
using HealthSync.Core.Enums;
using HealthSync.Core.Entities;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Core.Services;

public class LabService : ILabService
{
    private readonly IUnitOfWork _uow;

    public LabService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // ---- Lab Test Catalog ----

    public async Task<List<LabTestResponseDto>> GetAllLabTestsAsync()
    {
        var tests = await _uow.LabTests.FindAsync(t => t.IsActive);
        return tests.Select(t => new LabTestResponseDto
        {
            Id = t.Id,
            TestName = t.TestName,
            Category = t.Category,
            Description = t.Description,
            Price = t.Price,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    public async Task<PaginatedResult<LabTestResponseDto>> GetLabTestsAsync(int page, int pageSize, string? search)
    {
        var query = _uow.LabTests.Query();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(t => t.TestName.ToLower().Contains(term)
                                  || (t.Category != null && t.Category.ToLower().Contains(term)));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(t => t.TestName)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PaginatedResult<LabTestResponseDto>
        {
            Items = items.Select(MapTestToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<LabTestResponseDto?> GetLabTestByIdAsync(Guid id)
    {
        var test = await _uow.LabTests.GetByIdAsync(id);
        return test == null ? null : MapTestToDto(test);
    }

    public async Task<LabTestResponseDto> CreateLabTestAsync(CreateLabTestDto dto)
    {
        var test = new LabTest
        {
            TestName = dto.TestName,
            Category = dto.Category,
            Description = dto.Description,
            Price = dto.Price
        };
        await _uow.LabTests.AddAsync(test);
        await _uow.SaveChangesAsync();
        return MapTestToDto(test);
    }

    public async Task<LabTestResponseDto?> UpdateLabTestAsync(Guid id, UpdateLabTestDto dto)
    {
        var test = await _uow.LabTests.GetByIdAsync(id);
        if (test == null) return null;

        if (dto.TestName != null) test.TestName = dto.TestName;
        if (dto.Category != null) test.Category = dto.Category;
        if (dto.Description != null) test.Description = dto.Description;
        if (dto.Price.HasValue) test.Price = dto.Price.Value;
        if (dto.IsActive.HasValue) test.IsActive = dto.IsActive.Value;
        test.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return MapTestToDto(test);
    }

    public async Task<bool> DeleteLabTestAsync(Guid id)
    {
        var test = await _uow.LabTests.GetByIdAsync(id);
        if (test == null) return false;
        await _uow.LabTests.DeleteAsync(test);
        await _uow.SaveChangesAsync();
        return true;
    }

    // ---- Lab Orders ----

    public async Task<List<LabOrderResponseDto>> GetAllLabOrdersAsync(LabOrderStatus? status = null, Guid? patientId = null, Guid? doctorId = null)
    {
        IQueryable<LabOrder> query = _uow.LabOrders.Query()
            .Include(o => o.Patient)
            .Include(o => o.Doctor)
            .Include(o => o.LabTest);

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        if (patientId.HasValue) query = query.Where(o => o.PatientId == patientId.Value);
        if (doctorId.HasValue) query = query.Where(o => o.DoctorId == doctorId.Value);

        var orders = await query.OrderByDescending(o => o.OrderedAt).ToListAsync();
        return orders.Select(MapOrderToDto).ToList();
    }

    public async Task<PaginatedResult<LabOrderResponseDto>> GetLabOrdersAsync(int page, int pageSize, LabOrderStatus? status, Guid? patientId, Guid? doctorId, string? search)
    {
        IQueryable<LabOrder> query = _uow.LabOrders.Query()
            .Include(o => o.Patient)
            .Include(o => o.Doctor)
            .Include(o => o.LabTest);

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        if (patientId.HasValue) query = query.Where(o => o.PatientId == patientId.Value);
        if (doctorId.HasValue) query = query.Where(o => o.DoctorId == doctorId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(o => o.Patient.FirstName.ToLower().Contains(term)
                                  || o.Patient.LastName.ToLower().Contains(term)
                                  || o.LabTest.TestName.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(o => o.OrderedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PaginatedResult<LabOrderResponseDto>
        {
            Items = items.Select(MapOrderToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<LabOrderResponseDto?> GetLabOrderByIdAsync(Guid id)
    {
        var order = await _uow.LabOrders.Query()
            .Include(o => o.Patient)
            .Include(o => o.Doctor)
            .Include(o => o.LabTest)
            .FirstOrDefaultAsync(o => o.Id == id);
        return order == null ? null : MapOrderToDto(order);
    }

    public async Task<LabOrderResponseDto> CreateLabOrderAsync(Guid doctorId, CreateLabOrderDto dto)
    {
        var order = new LabOrder
        {
            PatientId = dto.PatientId,
            DoctorId = doctorId,
            LabTestId = dto.LabTestId,
            Notes = dto.Notes
        };
        await _uow.LabOrders.AddAsync(order);
        await _uow.SaveChangesAsync();
        return (await GetLabOrderByIdAsync(order.Id))!;
    }

    public async Task<LabOrderResponseDto?> UpdateLabOrderAsync(Guid id, UpdateLabOrderDto dto)
    {
        var order = await _uow.LabOrders.Query()
            .Include(o => o.Patient)
            .Include(o => o.Doctor)
            .Include(o => o.LabTest)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return null;

        if (dto.Status.HasValue)
        {
            order.Status = dto.Status.Value;
            if (dto.Status.Value == LabOrderStatus.Completed)
                order.CompletedAt = DateTime.UtcNow;
        }
        if (dto.Result != null) order.Result = dto.Result;
        if (dto.ResultSummary != null) order.ResultSummary = dto.ResultSummary;
        if (dto.Notes != null) order.Notes = dto.Notes;
        if (dto.ReferenceRange != null) order.ReferenceRange = dto.ReferenceRange;
        order.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return MapOrderToDto(order);
    }

    public async Task<bool> DeleteLabOrderAsync(Guid id)
    {
        var order = await _uow.LabOrders.GetByIdAsync(id);
        if (order == null) return false;
        await _uow.LabOrders.DeleteAsync(order);
        await _uow.SaveChangesAsync();
        return true;
    }

    // ---- Mapping ----

    private static LabTestResponseDto MapTestToDto(LabTest t) => new()
    {
        Id = t.Id,
        TestName = t.TestName,
        Category = t.Category,
        Description = t.Description,
        Price = t.Price,
        IsActive = t.IsActive,
        CreatedAt = t.CreatedAt
    };

    private static LabOrderResponseDto MapOrderToDto(LabOrder o) => new()
    {
        Id = o.Id,
        PatientId = o.PatientId,
        PatientName = $"{o.Patient.FirstName} {o.Patient.LastName}",
        DoctorId = o.DoctorId,
        DoctorName = $"Dr. {o.Doctor.FirstName} {o.Doctor.LastName}",
        LabTestId = o.LabTestId,
        TestName = o.LabTest.TestName,
        Category = o.LabTest.Category,
        Price = o.LabTest.Price,
        Status = o.Status,
        Result = o.Result,
        ResultSummary = o.ResultSummary,
        Notes = o.Notes,
        ReferenceRange = o.ReferenceRange,
        CompletedAt = o.CompletedAt,
        OrderedAt = o.OrderedAt
    };
}
