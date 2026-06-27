using Microsoft.EntityFrameworkCore;
using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.Patient;
using HealthSync.Core.Entities;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Core.Services;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _uow;

    public PatientService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PaginatedResult<PatientResponseDto>> GetAllAsync(int page, int pageSize, string? search)
    {
        var query = _uow.Patients.Query();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p => p.FirstName.ToLower().Contains(term)
                                  || p.LastName.ToLower().Contains(term)
                                  || p.Phone.Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PaginatedResult<PatientResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PatientResponseDto?> GetByIdAsync(Guid id)
    {
        var patient = await _uow.Patients.GetByIdAsync(id);
        return patient == null ? null : MapToDto(patient);
    }

    public async Task<PatientResponseDto> CreateAsync(CreatePatientDto dto)
    {
        var patient = new Patient
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = DateOnly.FromDateTime(dto.DateOfBirth),
            Gender = dto.Gender,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            BloodType = dto.BloodType,
            EmergencyContact = dto.EmergencyContact,
            EmergencyPhone = dto.EmergencyPhone,
            MedicalHistory = dto.MedicalHistory,
            Allergies = dto.Allergies
        };

        await _uow.Patients.AddAsync(patient);
        await _uow.SaveChangesAsync();
        return MapToDto(patient);
    }

    public async Task<PatientResponseDto?> UpdateAsync(Guid id, UpdatePatientDto dto)
    {
        var patient = await _uow.Patients.GetByIdAsync(id);
        if (patient == null) return null;

        if (dto.FirstName != null) patient.FirstName = dto.FirstName;
        if (dto.LastName != null) patient.LastName = dto.LastName;
        if (dto.Phone != null) patient.Phone = dto.Phone;
        if (dto.Email != null) patient.Email = dto.Email;
        if (dto.Address != null) patient.Address = dto.Address;
        if (dto.EmergencyContact != null) patient.EmergencyContact = dto.EmergencyContact;
        if (dto.EmergencyPhone != null) patient.EmergencyPhone = dto.EmergencyPhone;
        if (dto.MedicalHistory != null) patient.MedicalHistory = dto.MedicalHistory;
        if (dto.Allergies != null) patient.Allergies = dto.Allergies;

        patient.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return MapToDto(patient);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var patient = await _uow.Patients.GetByIdAsync(id);
        if (patient == null) return false;

        await _uow.Patients.DeleteAsync(patient);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<PaginatedResult<PatientResponseDto>> GetByDoctorIdAsync(Guid doctorId, int page, int pageSize, string? search)
    {
        var query = _uow.Patients.Query()
            .Where(p => p.Appointments.Any(a => a.DoctorId == doctorId));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p => p.FirstName.ToLower().Contains(term)
                                  || p.LastName.ToLower().Contains(term)
                                  || p.Phone.Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PaginatedResult<PatientResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<PatientResponseDto>> SearchAsync(string query)
    {
        var term = query.ToLower();
        var patients = await _uow.Patients.FindAsync(p =>
            p.FirstName.ToLower().Contains(term) ||
            p.LastName.ToLower().Contains(term) ||
            p.Phone.Contains(term));

        return patients.Take(10).Select(MapToDto);
    }

    private static PatientResponseDto MapToDto(Patient p) => new()
    {
        Id = p.Id,
        FirstName = p.FirstName,
        LastName = p.LastName,
        DateOfBirth = p.DateOfBirth.ToDateTime(TimeOnly.MinValue),
        Gender = p.Gender,
        Phone = p.Phone,
        Email = p.Email,
        Address = p.Address,
        BloodType = p.BloodType,
        EmergencyContact = p.EmergencyContact,
        EmergencyPhone = p.EmergencyPhone,
        MedicalHistory = p.MedicalHistory,
        Allergies = p.Allergies,
        CreatedAt = p.CreatedAt
    };
}
