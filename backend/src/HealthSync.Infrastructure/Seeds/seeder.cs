using HealthSync.Core.Entities;
using HealthSync.Core.Entities.Identity;
using HealthSync.Core.Enums;
using HealthSync.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthSync.Infrastructure.Seeds;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, PasswordHasher<ApplicationUser> hasher)
    {
        await context.Database.MigrateAsync();

        // Seed admin user
        if (!await context.Users.AnyAsync(u => u.UserName == "admin"))
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@healthsync.com",
                FirstName = "System",
                LastName = "Admin",
                Role = UserRole.Admin,
                IsActive = true
            };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
            context.Users.Add(admin);
        }

        // Seed sample doctor
        if (!await context.Users.AnyAsync(u => u.UserName == "dr.smith"))
        {
            var doctorUser = new ApplicationUser
            {
                UserName = "dr.smith",
                Email = "dr.smith@healthsync.com",
                FirstName = "John",
                LastName = "Smith",
                Role = UserRole.Doctor,
                IsActive = true
            };
            doctorUser.PasswordHash = hasher.HashPassword(doctorUser, "Doctor@123");
            context.Users.Add(doctorUser);

            // Need to save to get the user Id
            await context.SaveChangesAsync();

            var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUser.Id);
            if (doctor == null)
            {
                context.Doctors.Add(new Doctor
                {
                    UserId = doctorUser.Id,
                    FirstName = "John",
                    LastName = "Smith",
                    Specialization = "General Practitioner",
                    LicenseNumber = "LIC-001",
                    ConsultationFee = 500,
                    IsActive = true
                });
            }
        }

        // Seed sample receptionist
        if (!await context.Users.AnyAsync(u => u.UserName == "reception"))
        {
            var receptionist = new ApplicationUser
            {
                UserName = "reception",
                Email = "reception@healthsync.com",
                FirstName = "Jane",
                LastName = "Doe",
                Role = UserRole.Receptionist,
                IsActive = true
            };
            receptionist.PasswordHash = hasher.HashPassword(receptionist, "Recept@123");
            context.Users.Add(receptionist);
        }

        // Seed sample pharmacist
        if (!await context.Users.AnyAsync(u => u.UserName == "pharmacist"))
        {
            var pharmacist = new ApplicationUser
            {
                UserName = "pharmacist",
                Email = "pharmacist@healthsync.com",
                FirstName = "Bob",
                LastName = "Johnson",
                Role = UserRole.Pharmacist,
                IsActive = true
            };
            pharmacist.PasswordHash = hasher.HashPassword(pharmacist, "Pharma@123");
            context.Users.Add(pharmacist);
        }

        // Seed sample medicines
        if (!await context.Medicines.AnyAsync())
        {
            context.Medicines.AddRange(
                new Medicine { Name = "Paracetamol", GenericName = "Acetaminophen", Category = "Analgesic", Unit = "tablet", Price = 2.50m, ReorderLevel = 100 },
                new Medicine { Name = "Amoxicillin", GenericName = "Amoxicillin", Category = "Antibiotic", Unit = "capsule", Price = 8.00m, ReorderLevel = 50 },
                new Medicine { Name = "Omeprazole", GenericName = "Omeprazole", Category = "Antacid", Unit = "capsule", Price = 12.00m, ReorderLevel = 30 },
                new Medicine { Name = "Salbutamol Inhaler", GenericName = "Salbutamol", Category = "Respiratory", Unit = "inhaler", Price = 150.00m, ReorderLevel = 10 },
                new Medicine { Name = "Cetirizine", GenericName = "Cetirizine HCl", Category = "Antihistamine", Unit = "tablet", Price = 3.00m, ReorderLevel = 80 }
            );
        }

        await context.SaveChangesAsync();
    }
}
