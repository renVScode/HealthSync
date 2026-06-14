using HealthSync.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.Property(d => d.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(d => d.LastName).HasMaxLength(100).IsRequired();
        builder.Property(d => d.Specialization).HasMaxLength(200).IsRequired();
        builder.Property(d => d.LicenseNumber).HasMaxLength(50).IsRequired();
        builder.Property(d => d.Phone).HasMaxLength(20);
        builder.Property(d => d.Email).HasMaxLength(255);
        builder.Property(d => d.ConsultationFee).HasColumnType("decimal(10,2)");
        builder.HasOne(d => d.User).WithOne(u => u.Doctor).HasForeignKey<Doctor>(d => d.UserId);
        builder.HasIndex(d => d.LicenseNumber).IsUnique();
    }
}
