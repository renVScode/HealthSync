using HealthSync.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthSync.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Gender).HasMaxLength(10).IsRequired();
        builder.Property(p => p.Phone).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(25);
        builder.Property(p => p.BloodType).HasMaxLength(5);
        builder.Property(p => p.EmergencyContact).HasMaxLength(100);
        builder.Property(p => p.EmergencyPhone).HasMaxLength(20);
        builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(p => new { p.LastName, p.FirstName });
        builder.HasIndex(p => p.Phone);
    }
}
