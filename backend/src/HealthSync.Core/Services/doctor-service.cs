using Microsoft.EntityFrameworkCore;
using HealthSync.Core.DTOs;
using HealthSync.Core.DTOs.Doctor;
using HealthSync.Core.Enums;
using HealthSync.Core.Entities;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Core.Services;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _uow;

    public DoctorService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<DoctorResponseDto>> GetAllAsync()
    {
        var doctors = await _uow.Doctors.GetAllAsync();
        return doctors.Select(d => MapToDto(d));
    }

    public async Task<PaginatedResult<DoctorResponseDto>> GetAllAsync(int page, int pageSize, string? search)
    {
        var query = _uow.Doctors.Query();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(d => d.FirstName.ToLower().Contains(term)
                                  || d.LastName.ToLower().Contains(term)
                                  || d.Specialization.ToLower().Contains(term)
                                  || d.LicenseNumber.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(d => d.LastName)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        return new PaginatedResult<DoctorResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<DoctorResponseDto?> GetByIdAsync(Guid id)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(id);
        return doctor == null ? null : new DoctorResponseDto
        {
            Id = doctor.Id,
            UserId = doctor.UserId,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            Specialization = doctor.Specialization,
            LicenseNumber = doctor.LicenseNumber,
            Phone = doctor.Phone,
            Email = doctor.Email,
            Bio = doctor.Bio,
            ConsultationFee = doctor.ConsultationFee,
            ProfileImageUrl = doctor.ProfileImageUrl,
            LicenseImageUrl = doctor.LicenseImageUrl,
            IsActive = doctor.IsActive
        };
    }

    public async Task<DoctorResponseDto?> GetByUserIdAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var parsedUserId))
            return null;
        var doctor = await _uow.Doctors.Query().FirstOrDefaultAsync(d => d.UserId == parsedUserId);
        return doctor == null ? null : MapToDto(doctor);
    }

    public async Task<DoctorResponseDto> CreateAsync(CreateDoctorDto dto)
    {
        var doctor = new Doctor
        {
            UserId = dto.UserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Specialization = dto.Specialization,
            LicenseNumber = dto.LicenseNumber,
            Phone = dto.Phone,
            Email = dto.Email,
            Bio = dto.Bio,
            ConsultationFee = dto.ConsultationFee
        };
        await _uow.Doctors.AddAsync(doctor);
        await _uow.SaveChangesAsync();
        return (await GetByIdAsync(doctor.Id))!;
    }

    public async Task<DoctorResponseDto?> UpdateAsync(Guid id, UpdateDoctorDto dto)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(id);
        if (doctor == null) return null;
        if (dto.FirstName != null) doctor.FirstName = dto.FirstName;
        if (dto.LastName != null) doctor.LastName = dto.LastName;
        if (dto.Specialization != null) doctor.Specialization = dto.Specialization;
        if (dto.Phone != null) doctor.Phone = dto.Phone;
        if (dto.Email != null) doctor.Email = dto.Email;
        if (dto.Bio != null) doctor.Bio = dto.Bio;
        if (dto.ConsultationFee.HasValue) doctor.ConsultationFee = dto.ConsultationFee.Value;
        if (dto.ProfileImageUrl != null) doctor.ProfileImageUrl = dto.ProfileImageUrl;
        if (dto.LicenseImageUrl != null) doctor.LicenseImageUrl = dto.LicenseImageUrl;
        if (dto.IsActive.HasValue) doctor.IsActive = dto.IsActive.Value;
        doctor.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
        return (await GetByIdAsync(id))!;
    }

    public async Task<List<AvailabilityResponseDto>> GetAvailabilityAsync(Guid doctorId)
    {
        var availabilities = await _uow.DoctorAvailabilities
            .FindAsync(a => a.DoctorId == doctorId);
        return availabilities.Select(a => new AvailabilityResponseDto
        {
            Id = a.Id,
            DayOfWeek = a.DayOfWeek,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            SlotDuration = a.SlotDuration,
            IsAvailable = a.IsAvailable
        }).ToList();
    }

    public async Task UpdateAvailabilityAsync(Guid doctorId, List<UpsertAvailabilityDto> dtos)
    {
        var existing = (await _uow.DoctorAvailabilities.FindAsync(a => a.DoctorId == doctorId)).ToList();
        foreach (var item in existing)
            await _uow.DoctorAvailabilities.DeleteAsync(item);

        foreach (var dto in dtos)
        {
            await _uow.DoctorAvailabilities.AddAsync(new DoctorAvailability
            {
                DoctorId = doctorId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsAvailable = dto.IsAvailable
            });
        }
        await _uow.SaveChangesAsync();
    }

    public async Task<List<TimeOffResponseDto>> GetTimeOffsAsync(Guid doctorId)
    {
        var timeOffs = await _uow.TimeOffs.FindAsync(t => t.DoctorId == doctorId);
        return timeOffs.Select(t => new TimeOffResponseDto
        {
            Id = t.Id,
            StartDate = t.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = t.EndDate.ToDateTime(TimeOnly.MinValue),
            Reason = t.Reason,
            IsApproved = t.IsApproved
        }).ToList();
    }

    public async Task<TimeOffResponseDto> RequestTimeOffAsync(Guid doctorId, CreateTimeOffDto dto)
    {
        var timeOff = new TimeOff
        {
            DoctorId = doctorId,
            StartDate = DateOnly.FromDateTime(dto.StartDate),
            EndDate = DateOnly.FromDateTime(dto.EndDate),
            Reason = dto.Reason
        };
        await _uow.TimeOffs.AddAsync(timeOff);
        await _uow.SaveChangesAsync();
        return new TimeOffResponseDto
        {
            Id = timeOff.Id,
            StartDate = timeOff.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = timeOff.EndDate.ToDateTime(TimeOnly.MinValue),
            Reason = timeOff.Reason,
            IsApproved = timeOff.IsApproved
        };
    }

    public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateTime date)
    {
        var dayOfWeek = date.DayOfWeek;
        var dateOnly = DateOnly.FromDateTime(date);
        var availabilities = await _uow.DoctorAvailabilities
            .FindAsync(a => a.DoctorId == doctorId && a.DayOfWeek == dayOfWeek && a.IsAvailable);

        var hasTimeOff = await _uow.TimeOffs.ExistsAsync(t =>
            t.DoctorId == doctorId && t.StartDate <= dateOnly && t.EndDate >= dateOnly && t.IsApproved);

        if (hasTimeOff || !availabilities.Any())
            return [];

        var existingAppointments = await _uow.Appointments.FindAsync(a =>
            a.DoctorId == doctorId &&
            a.StartTime.Date == date.Date &&
            a.Status != AppointmentStatus.Cancelled);

        var slots = new List<AvailableSlotDto>();
        foreach (var availability in availabilities)
        {
            var current = date.Date + availability.StartTime.ToTimeSpan();
            var end = date.Date + availability.EndTime.ToTimeSpan();

            while (current.AddMinutes(15) <= end)
            {
                var slotEnd = current.AddMinutes(15);
                var isBooked = existingAppointments.Any(a => a.StartTime < slotEnd && a.EndTime > current);
                slots.Add(new AvailableSlotDto
                {
                    StartTime = current,
                    EndTime = slotEnd,
                    IsAvailable = !isBooked
                });
                current = slotEnd;
            }
        }
        return slots;
    }

    private static DoctorResponseDto MapToDto(Doctor d) => new()
    {
        Id = d.Id,
        UserId = d.UserId,
        FirstName = d.FirstName,
        LastName = d.LastName,
        Specialization = d.Specialization,
        LicenseNumber = d.LicenseNumber,
        Phone = d.Phone,
        Email = d.Email,
        Bio = d.Bio,
        ConsultationFee = d.ConsultationFee,
        ProfileImageUrl = d.ProfileImageUrl,
        LicenseImageUrl = d.LicenseImageUrl,
        IsActive = d.IsActive
    };
}
