using HealthSync.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Reason).HasMaxLength(500);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);
        builder.HasOne(a => a.Patient).WithMany(p => p.Appointments).HasForeignKey(a => a.PatientId);
        builder.HasOne(a => a.Doctor).WithMany(d => d.Appointments).HasForeignKey(a => a.DoctorId);
        builder.HasOne(a => a.MedicalRecord).WithOne(r => r.Appointment).HasForeignKey<MedicalRecord>(r => r.AppointmentId);
        builder.HasOne(a => a.Billing).WithOne(b => b.Appointment).HasForeignKey<Billing>(b => b.AppointmentId);
        builder.HasIndex(a => new { a.DoctorId, a.StartTime });
        builder.HasIndex(a => a.PatientId);
        builder.HasIndex(a => a.Status);
    }
}
