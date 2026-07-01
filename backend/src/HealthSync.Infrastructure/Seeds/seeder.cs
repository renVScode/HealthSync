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

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // ── USERS ──────────────────────────────────────────────────────────

        if (!await context.Users.AnyAsync(u => u.UserName == "admin"))
        {
            context.Users.Add(new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@healthsync.com",
                FirstName = "System",
                LastName = "Admin",
                Role = UserRole.Admin,
                IsActive = true,
                PasswordHash = hasher.HashPassword(new ApplicationUser(), "Admin@123")
            });
        }

        if (!await context.Users.AnyAsync(u => u.UserName == "dr.smith"))
        {
            var drSmithUser = new ApplicationUser
            {
                UserName = "dr.smith",
                Email = "dr.smith@healthsync.com",
                FirstName = "John",
                LastName = "Smith",
                Role = UserRole.Doctor,
                IsActive = true,
                PasswordHash = hasher.HashPassword(new ApplicationUser(), "Doctor@123")
            };
            context.Users.Add(drSmithUser);
            await context.SaveChangesAsync();

            if (!await context.Doctors.AnyAsync(d => d.UserId == drSmithUser.Id))
            {
                context.Doctors.Add(new Doctor
                {
                    UserId = drSmithUser.Id,
                    FirstName = "John",
                    LastName = "Smith",
                    Specialization = "General Practitioner",
                    LicenseNumber = "LIC-001",
                    ConsultationFee = 500,
                    Phone = "09171234567",
                    Email = "dr.smith@healthsync.com",
                    Bio = "Experienced general practitioner with 10 years of clinical experience.",
                    IsActive = true
                });
            }
        }

        if (!await context.Users.AnyAsync(u => u.UserName == "reception"))
        {
            context.Users.Add(new ApplicationUser
            {
                UserName = "reception",
                Email = "reception@healthsync.com",
                FirstName = "Jane",
                LastName = "Doe",
                Role = UserRole.Receptionist,
                IsActive = true,
                PasswordHash = hasher.HashPassword(new ApplicationUser(), "Recept@123")
            });
        }

        if (!await context.Users.AnyAsync(u => u.UserName == "pharmacist"))
        {
            context.Users.Add(new ApplicationUser
            {
                UserName = "pharmacist",
                Email = "pharmacist@healthsync.com",
                FirstName = "Bob",
                LastName = "Johnson",
                Role = UserRole.Pharmacist,
                IsActive = true,
                PasswordHash = hasher.HashPassword(new ApplicationUser(), "Pharma@123")
            });
        }

        if (!await context.Users.AnyAsync(u => u.UserName == "dr.chen"))
        {
            var drChenUser = new ApplicationUser
            {
                UserName = "dr.chen",
                Email = "dr.chen@healthsync.com",
                FirstName = "Emily",
                LastName = "Chen",
                Role = UserRole.Doctor,
                IsActive = true,
                PasswordHash = hasher.HashPassword(new ApplicationUser(), "Doctor@123")
            };
            context.Users.Add(drChenUser);
            await context.SaveChangesAsync();

            if (!await context.Doctors.AnyAsync(d => d.UserId == drChenUser.Id))
            {
                context.Doctors.Add(new Doctor
                {
                    UserId = drChenUser.Id,
                    FirstName = "Emily",
                    LastName = "Chen",
                    Specialization = "Cardiologist",
                    LicenseNumber = "LIC-002",
                    ConsultationFee = 800,
                    Phone = "09179876543",
                    Email = "dr.chen@healthsync.com",
                    Bio = "Board-certified cardiologist specializing in preventive cardiology.",
                    IsActive = true
                });
            }
        }

        await context.SaveChangesAsync();

        // ── DOCTOR AVAILABILITIES ─────────────────────────────────────────
        // Both doctors: Mon–Fri, 9:00–17:00

        if (!await context.DoctorAvailabilities.AnyAsync())
        {
            var doctors = await context.Doctors.ToListAsync();

            foreach (var doctor in doctors)
            {
                for (var day = DayOfWeek.Monday; day <= DayOfWeek.Friday; day++)
                {
                    context.DoctorAvailabilities.Add(new DoctorAvailability
                    {
                        DoctorId = doctor.Id,
                        DayOfWeek = day,
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(17, 0),
                        SlotDuration = 15,
                        IsAvailable = true
                    });
                }
            }
        }

        await context.SaveChangesAsync();

        // ── PATIENTS ───────────────────────────────────────────────────────

        if (!await context.Patients.AnyAsync())
        {
            var patients = new List<Patient>
            {
                new()
                {
                    FirstName = "Juan", LastName = "Dela Cruz", DateOfBirth = new DateOnly(1990, 5, 12),
                    Gender = "Male", Phone = "09150001111", Email = "juan.delacruz@email.com",
                    Address = "123 Rizal St, Manila", BloodType = "O+",
                    EmergencyContact = "Maria Dela Cruz", EmergencyPhone = "09150001112",
                    Allergies = "Penicillin"
                },
                new()
                {
                    FirstName = "Maria", LastName = "Santos", DateOfBirth = new DateOnly(1985, 8, 23),
                    Gender = "Female", Phone = "09150002222", Email = "maria.santos@email.com",
                    Address = "456 Mabini Ave, Quezon City", BloodType = "A+",
                    EmergencyContact = "Pedro Santos", EmergencyPhone = "09150002223"
                },
                new()
                {
                    FirstName = "Pedro", LastName = "Gonzales", DateOfBirth = new DateOnly(1978, 3, 7),
                    Gender = "Male", Phone = "09150003333", Email = "pedro.gonzales@email.com",
                    Address = "789 Bonifacio St, Makati", BloodType = "B+",
                    EmergencyContact = "Ana Gonzales", EmergencyPhone = "09150003334",
                    MedicalHistory = "Type 2 Diabetes"
                },
                new()
                {
                    FirstName = "Ana", LastName = "Reyes", DateOfBirth = new DateOnly(1995, 11, 19),
                    Gender = "Female", Phone = "09150004444", Email = "ana.reyes@email.com",
                    Address = "321 Luna St, Pasig", BloodType = "AB+",
                    EmergencyContact = "Carlos Reyes", EmergencyPhone = "09150004445"
                },
                new()
                {
                    FirstName = "Carlos", LastName = "Mendoza", DateOfBirth = new DateOnly(2000, 1, 30),
                    Gender = "Male", Phone = "09150005555", Email = "carlos.mendoza@email.com",
                    Address = "654 Aguinaldo St, Mandaluyong", BloodType = "O-",
                    EmergencyContact = "Sofia Mendoza", EmergencyPhone = "09150005556",
                    Allergies = "Sulfa Drugs"
                },
                new()
                {
                    FirstName = "Sofia", LastName = "Villanueva", DateOfBirth = new DateOnly(1982, 7, 15),
                    Gender = "Female", Phone = "09150006666", Email = "sofia.villanueva@email.com",
                    Address = "987 Quezon Blvd, Taguig", BloodType = "A-",
                    EmergencyContact = "Miguel Villanueva", EmergencyPhone = "09150006667",
                    MedicalHistory = "Hypertension"
                },
                new()
                {
                    FirstName = "Miguel", LastName = "Lopez", DateOfBirth = new DateOnly(1992, 4, 2),
                    Gender = "Male", Phone = "09150007777", Email = "miguel.lopez@email.com",
                    Address = "147 Paterno St, Makati", BloodType = "B-",
                    EmergencyContact = "Isabela Lopez", EmergencyPhone = "09150007778"
                },
                new()
                {
                    FirstName = "Isabela", LastName = "Gomez", DateOfBirth = new DateOnly(2003, 9, 8),
                    Gender = "Female", Phone = "09150008888", Email = "isabela.gomez@email.com",
                    Address = "258 Mabini St, Manila", BloodType = "O+",
                    EmergencyContact = "Jose Gomez", EmergencyPhone = "09150008889",
                    Allergies = "Latex"
                },
                new()
                {
                    FirstName = "Jose", LastName = "Rizal", DateOfBirth = new DateOnly(1975, 6, 19),
                    Gender = "Male", Phone = "09150009999", Email = "jose.rizal@email.com",
                    Address = "369 Katipunan Ave, Quezon City", BloodType = "AB-",
                    EmergencyContact = "Elena Rizal", EmergencyPhone = "09150009990",
                    MedicalHistory = "Asthma"
                },
                new()
                {
                    FirstName = "Elena", LastName = "Torres", DateOfBirth = new DateOnly(1988, 12, 25),
                    Gender = "Female", Phone = "09150001010", Email = "elena.torres@email.com",
                    Address = "741 Shaw Blvd, Pasig", BloodType = "A+",
                    EmergencyContact = "Andres Torres", EmergencyPhone = "09150001011"
                },
                new()
                {
                    FirstName = "Andres", LastName = "Bonifacio", DateOfBirth = new DateOnly(1998, 11, 30),
                    Gender = "Male", Phone = "09150001111", Email = "andres.bonifacio@email.com",
                    Address = "852 EDSA, Mandaluyong", BloodType = "O+",
                    EmergencyContact = "Gabriela Bonifacio", EmergencyPhone = "09150001112"
                },
                new()
                {
                    FirstName = "Gabriela", LastName = "Silang", DateOfBirth = new DateOnly(2001, 3, 17),
                    Gender = "Female", Phone = "09150001212", Email = "gabriela.silang@email.com",
                    Address = "963 Taft Ave, Manila", BloodType = "B+",
                    EmergencyContact = "Diego Silang", EmergencyPhone = "09150001213",
                    Allergies = "Ibuprofen"
                }
            };

            context.Patients.AddRange(patients);
            await context.SaveChangesAsync();
        }

        // ── LAB TESTS CATALOG ──────────────────────────────────────────────

        if (!await context.LabTests.AnyAsync())
        {
            context.LabTests.AddRange(
                new LabTest { TestName = "Complete Blood Count (CBC)", Category = "Hematology", Description = "Measures WBC, RBC, hemoglobin, hematocrit, and platelets", Price = 350m, IsActive = true },
                new LabTest { TestName = "Urinalysis", Category = "Clinical Chemistry", Description = "Physical, chemical, and microscopic examination of urine", Price = 200m, IsActive = true },
                new LabTest { TestName = "Fecalysis", Category = "Clinical Chemistry", Description = "Microscopic examination of stool sample", Price = 250m, IsActive = true },
                new LabTest { TestName = "Lipid Panel", Category = "Clinical Chemistry", Description = "Total cholesterol, HDL, LDL, and triglycerides", Price = 800m, IsActive = true },
                new LabTest { TestName = "Fasting Blood Sugar", Category = "Clinical Chemistry", Description = "Blood glucose level after 8-hour fast", Price = 150m, IsActive = true },
                new LabTest { TestName = "HbA1c", Category = "Clinical Chemistry", Description = "Glycated hemoglobin for 3-month glucose average", Price = 600m, IsActive = true },
                new LabTest { TestName = "Creatinine", Category = "Clinical Chemistry", Description = "Kidney function test measuring creatinine level", Price = 200m, IsActive = true },
                new LabTest { TestName = "BUN", Category = "Clinical Chemistry", Description = "Blood Urea Nitrogen for kidney function", Price = 180m, IsActive = true },
                new LabTest { TestName = "ALT / SGPT", Category = "Liver Function", Description = "Alanine aminotransferase for liver health", Price = 250m, IsActive = true },
                new LabTest { TestName = "AST / SGOT", Category = "Liver Function", Description = "Aspartate aminotransferase for liver health", Price = 250m, IsActive = true },
                new LabTest { TestName = "TSH", Category = "Thyroid", Description = "Thyroid Stimulating Hormone", Price = 500m, IsActive = true },
                new LabTest { TestName = "Free T4", Category = "Thyroid", Description = "Free thyroxine level", Price = 450m, IsActive = false },
                new LabTest { TestName = "Vitamin D (25-OH)", Category = "Immunology", Description = "Vitamin D level screening", Price = 1200m, IsActive = true },
                new LabTest { TestName = "Troponin I", Category = "Cardiac Markers", Description = "Cardiac troponin for heart attack diagnosis", Price = 1500m, IsActive = false }
            );
            await context.SaveChangesAsync();
        }

        // ── MEDICINES ──────────────────────────────────────────────────────

        if (!await context.Medicines.AnyAsync())
        {
            context.Medicines.AddRange(
                new Medicine { Name = "Paracetamol", GenericName = "Acetaminophen", Category = "Analgesic", Unit = "tablet", Price = 2.50m, ReorderLevel = 100 },
                new Medicine { Name = "Amoxicillin", GenericName = "Amoxicillin", Category = "Antibiotic", Unit = "capsule", Price = 8.00m, ReorderLevel = 50 },
                new Medicine { Name = "Omeprazole", GenericName = "Omeprazole", Category = "Antacid", Unit = "capsule", Price = 12.00m, ReorderLevel = 30 },
                new Medicine { Name = "Salbutamol Inhaler", GenericName = "Salbutamol", Category = "Respiratory", Unit = "inhaler", Price = 150.00m, ReorderLevel = 10 },
                new Medicine { Name = "Cetirizine", GenericName = "Cetirizine HCl", Category = "Antihistamine", Unit = "tablet", Price = 3.00m, ReorderLevel = 80 },
                new Medicine { Name = "Metformin", GenericName = "Metformin HCl", Category = "Antidiabetic", Unit = "tablet", Price = 5.00m, ReorderLevel = 50 },
                new Medicine { Name = "Losartan", GenericName = "Losartan Potassium", Category = "Antihypertensive", Unit = "tablet", Price = 7.50m, ReorderLevel = 60 },
                new Medicine { Name = "Atorvastatin", GenericName = "Atorvastatin Calcium", Category = "Cholesterol", Unit = "tablet", Price = 15.00m, ReorderLevel = 40 },
                new Medicine { Name = "Ibuprofen", GenericName = "Ibuprofen", Category = "NSAID", Unit = "tablet", Price = 3.50m, ReorderLevel = 100 },
                new Medicine { Name = "Co-Amoxiclav", GenericName = "Amoxicillin + Clavulanic Acid", Category = "Antibiotic", Unit = "tablet", Price = 25.00m, ReorderLevel = 30 }
            );
            await context.SaveChangesAsync();
        }

        // ── ADDITIONAL MEDICINES ────────────────────────────────────────────
        // If the Medicines block was skipped (existing data), ensure new drugs exist

        if (!await context.Medicines.AnyAsync(m => m.Name == "Losartan"))
        {
            context.Medicines.Add(new Medicine { Name = "Losartan", GenericName = "Losartan Potassium", Category = "Antihypertensive", Unit = "tablet", Price = 7.50m, ReorderLevel = 60 });
        }
        if (!await context.Medicines.AnyAsync(m => m.Name == "Atorvastatin"))
        {
            context.Medicines.Add(new Medicine { Name = "Atorvastatin", GenericName = "Atorvastatin Calcium", Category = "Cholesterol", Unit = "tablet", Price = 15.00m, ReorderLevel = 40 });
        }
        if (!await context.Medicines.AnyAsync(m => m.Name == "Ibuprofen"))
        {
            context.Medicines.Add(new Medicine { Name = "Ibuprofen", GenericName = "Ibuprofen", Category = "NSAID", Unit = "tablet", Price = 3.50m, ReorderLevel = 100 });
        }
        if (!await context.Medicines.AnyAsync(m => m.Name == "Co-Amoxiclav"))
        {
            context.Medicines.Add(new Medicine { Name = "Co-Amoxiclav", GenericName = "Amoxicillin + Clavulanic Acid", Category = "Antibiotic", Unit = "tablet", Price = 25.00m, ReorderLevel = 30 });
        }
        await context.SaveChangesAsync();

        // ── DOCTOR SERVICE OFFERINGS ─────────────────────────────────────────

        if (!await context.DoctorServiceOfferings.AnyAsync())
        {
            var doctors = await context.Doctors.ToListAsync();
            var smithId = doctors.First(d => d.LicenseNumber == "LIC-001").Id;
            var chenId = doctors.First(d => d.LicenseNumber == "LIC-002").Id;

            context.DoctorServiceOfferings.AddRange(
                new DoctorServiceOffering { DoctorId = smithId, ServiceName = "General Check-up", Description = "Comprehensive annual physical examination", Price = 500m, IsActive = true },
                new DoctorServiceOffering { DoctorId = smithId, ServiceName = "ECG", Description = "Resting 12-lead electrocardiogram", Price = 350m, IsActive = true },
                new DoctorServiceOffering { DoctorId = smithId, ServiceName = "Vaccination", Description = "Flu shot or routine vaccination", Price = 200m, IsActive = true },
                new DoctorServiceOffering { DoctorId = smithId, ServiceName = "Wound Dressing", Description = "Minor wound cleaning and dressing", Price = 150m, IsActive = true },
                new DoctorServiceOffering { DoctorId = smithId, ServiceName = "Nebulization", Description = "Respiratory treatment via nebulizer", Price = 300m, IsActive = true },
                new DoctorServiceOffering { DoctorId = smithId, ServiceName = "Medical Certificate", Description = "Fit-to-work or school medical cert", Price = 100m, IsActive = false },
                new DoctorServiceOffering { DoctorId = chenId, ServiceName = "Cardiac Consultation", Description = "Comprehensive cardiac evaluation", Price = 800m, IsActive = true },
                new DoctorServiceOffering { DoctorId = chenId, ServiceName = "Stress Test", Description = "Treadmill exercise stress test", Price = 1500m, IsActive = true },
                new DoctorServiceOffering { DoctorId = chenId, ServiceName = "Holter Monitor", Description = "24-hour Holter monitoring setup and interpretation", Price = 2000m, IsActive = true },
                new DoctorServiceOffering { DoctorId = chenId, ServiceName = "Echocardiogram", Description = "2D echo with Doppler", Price = 2500m, IsActive = true },
                new DoctorServiceOffering { DoctorId = chenId, ServiceName = "ECG", Description = "Resting 12-lead ECG", Price = 500m, IsActive = true },
                new DoctorServiceOffering { DoctorId = chenId, ServiceName = "Telemedicine Follow-up", Description = "Virtual follow-up consultation via video call", Price = 400m, IsActive = false }
            );
            await context.SaveChangesAsync();
        }

        // ── INVENTORY BATCHES ──────────────────────────────────────────────

        if (!await context.InventoryBatches.AnyAsync())
        {
            var meds = await context.Medicines.ToListAsync();
            var paracetamol = meds.First(m => m.Name == "Paracetamol");
            var amoxicillin = meds.First(m => m.Name == "Amoxicillin");
            var omeprazole = meds.First(m => m.Name == "Omeprazole");
            var salbutamol = meds.First(m => m.Name == "Salbutamol Inhaler");
            var cetirizine = meds.First(m => m.Name == "Cetirizine");
            var metformin = meds.First(m => m.Name == "Metformin");
            var losartan = meds.First(m => m.Name == "Losartan");
            var atorvastatin = meds.First(m => m.Name == "Atorvastatin");
            var ibuprofen = meds.First(m => m.Name == "Ibuprofen");
            var coamoxiclav = meds.First(m => m.Name == "Co-Amoxiclav");

            var batches = new List<InventoryBatch>
            {
                new() { MedicineId = paracetamol.Id, BatchNumber = "PCM-2026-A01", Quantity = 500, UnitPrice = 1.80m, ManufactureDate = new DateOnly(2026, 1, 15), ExpiryDate = new DateOnly(2028, 1, 15), Supplier = "PharmaCorp Inc.", Location = "Shelf A-1" },
                new() { MedicineId = paracetamol.Id, BatchNumber = "PCM-2026-A02", Quantity = 200, UnitPrice = 2.00m, ManufactureDate = new DateOnly(2026, 3, 1), ExpiryDate = new DateOnly(2028, 3, 1), Supplier = "MedSupply Co.", Location = "Shelf A-1" },
                new() { MedicineId = amoxicillin.Id, BatchNumber = "AMX-2026-B01", Quantity = 300, UnitPrice = 5.50m, ManufactureDate = new DateOnly(2026, 2, 10), ExpiryDate = new DateOnly(2027, 8, 10), Supplier = "PharmaCorp Inc.", Location = "Shelf B-2" },
                new() { MedicineId = amoxicillin.Id, BatchNumber = "AMX-2026-B02", Quantity = 150, UnitPrice = 6.00m, ManufactureDate = new DateOnly(2026, 4, 5), ExpiryDate = new DateOnly(2027, 10, 5), Supplier = "HealthPlus Distributors", Location = "Shelf B-2" },
                new() { MedicineId = omeprazole.Id, BatchNumber = "OME-2026-C01", Quantity = 200, UnitPrice = 9.00m, ManufactureDate = new DateOnly(2026, 1, 20), ExpiryDate = new DateOnly(2028, 7, 20), Supplier = "MedSupply Co.", Location = "Shelf C-1" },
                new() { MedicineId = metformin.Id, BatchNumber = "MET-2026-F01", Quantity = 500, UnitPrice = 3.50m, ManufactureDate = new DateOnly(2026, 2, 1), ExpiryDate = new DateOnly(2028, 2, 1), Supplier = "PharmaCorp Inc.", Location = "Shelf F-1" },
                new() { MedicineId = metformin.Id, BatchNumber = "MET-2026-F02", Quantity = 300, UnitPrice = 4.00m, ManufactureDate = new DateOnly(2026, 5, 15), ExpiryDate = new DateOnly(2028, 5, 15), Supplier = "MedSupply Co.", Location = "Shelf F-1" },
                new() { MedicineId = losartan.Id, BatchNumber = "LOS-2026-G01", Quantity = 400, UnitPrice = 5.50m, ManufactureDate = new DateOnly(2026, 3, 10), ExpiryDate = new DateOnly(2028, 3, 10), Supplier = "CardioCare Pharma", Location = "Shelf G-1" },
                new() { MedicineId = losartan.Id, BatchNumber = "LOS-2026-G02", Quantity = 200, UnitPrice = 6.00m, ManufactureDate = new DateOnly(2026, 6, 1), ExpiryDate = new DateOnly(2028, 6, 1), Supplier = "HealthPlus Distributors", Location = "Shelf G-1" },
                new() { MedicineId = atorvastatin.Id, BatchNumber = "ATV-2026-H01", Quantity = 350, UnitPrice = 11.00m, ManufactureDate = new DateOnly(2026, 1, 5), ExpiryDate = new DateOnly(2028, 1, 5), Supplier = "CardioCare Pharma", Location = "Shelf H-1" },
                new() { MedicineId = atorvastatin.Id, BatchNumber = "ATV-2026-H02", Quantity = 150, UnitPrice = 12.50m, ManufactureDate = new DateOnly(2025, 12, 1), ExpiryDate = new DateOnly(2027, 6, 1), Supplier = "PharmaCorp Inc.", Location = "Shelf H-1" },
                new() { MedicineId = ibuprofen.Id, BatchNumber = "IBU-2026-I01", Quantity = 600, UnitPrice = 2.20m, ManufactureDate = new DateOnly(2026, 4, 20), ExpiryDate = new DateOnly(2028, 10, 20), Supplier = "MedSupply Co.", Location = "Shelf I-1" },
                new() { MedicineId = coamoxiclav.Id, BatchNumber = "AMC-2026-J01", Quantity = 200, UnitPrice = 18.00m, ManufactureDate = new DateOnly(2026, 3, 1), ExpiryDate = new DateOnly(2028, 3, 1), Supplier = "PharmaCorp Inc.", Location = "Shelf J-1" },
            };

            // Add existing batches for omeprazole/salbutamol/cetirizine
            batches.Add(new() { MedicineId = omeprazole.Id, BatchNumber = "OME-2026-C02", Quantity = 80, UnitPrice = 10.00m, ManufactureDate = new DateOnly(2025, 11, 1), ExpiryDate = new DateOnly(2027, 5, 1), Supplier = "PharmaCorp Inc.", Location = "Shelf C-1" });
            batches.Add(new() { MedicineId = salbutamol.Id, BatchNumber = "SAL-2026-D01", Quantity = 30, UnitPrice = 120.00m, ManufactureDate = new DateOnly(2026, 3, 15), ExpiryDate = new DateOnly(2027, 9, 15), Supplier = "RespireMed Ltd.", Location = "Shelf D-1" });
            batches.Add(new() { MedicineId = cetirizine.Id, BatchNumber = "CTZ-2026-E01", Quantity = 400, UnitPrice = 2.00m, ManufactureDate = new DateOnly(2026, 5, 1), ExpiryDate = new DateOnly(2028, 5, 1), Supplier = "HealthPlus Distributors", Location = "Shelf E-1" });
            batches.Add(new() { MedicineId = cetirizine.Id, BatchNumber = "CTZ-2026-E02", Quantity = 250, UnitPrice = 2.20m, ManufactureDate = new DateOnly(2025, 8, 1), ExpiryDate = new DateOnly(2027, 2, 1), Supplier = "PharmaCorp Inc.", Location = "Shelf E-1" });

            context.InventoryBatches.AddRange(batches);
            await context.SaveChangesAsync();
        }

        // ── APPOINTMENTS ───────────────────────────────────────────────────
        // Total: 58 appointments — 13 in June (stable ref week) + 45 across July 2026

        if (!await context.Appointments.AnyAsync())
        {
            var doctors = await context.Doctors.ToListAsync();
            var patients = await context.Patients.ToListAsync();
            var services = await context.DoctorServiceOfferings.ToListAsync();

            var smithId = doctors.First(d => d.LicenseNumber == "LIC-001").Id;
            var chenId = doctors.First(d => d.LicenseNumber == "LIC-002").Id;

            var smithCheckup = services.FirstOrDefault(s => s.DoctorId == smithId && s.ServiceName == "General Check-up");
            var smithECG = services.FirstOrDefault(s => s.DoctorId == smithId && s.ServiceName == "ECG");
            var smithVaccine = services.FirstOrDefault(s => s.DoctorId == smithId && s.ServiceName == "Vaccination");
            var chenConsult = services.FirstOrDefault(s => s.DoctorId == chenId && s.ServiceName == "Cardiac Consultation");
            var chenECG = services.FirstOrDefault(s => s.DoctorId == chenId && s.ServiceName == "ECG");
            var chenStress = services.FirstOrDefault(s => s.DoctorId == chenId && s.ServiceName == "Stress Test");
            var chenEcho = services.FirstOrDefault(s => s.DoctorId == chenId && s.ServiceName == "Echocardiogram");

            var refMonday = new DateTime(2026, 6, 22, 0, 0, 0, DateTimeKind.Utc);
            var july1 = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc); // Wednesday
            var july6 = new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc); // Monday

            var appointments = new List<Appointment>();

            // ── JUNE 22-26 (13 appointments, stable reference week) ────

            // Mon 22 Jun – Dr. Smith
            appointments.Add(new() { PatientId = patients[0].Id, DoctorId = smithId, StartTime = refMonday.AddHours(9), EndTime = refMonday.AddHours(9.25), Status = AppointmentStatus.Completed, Reason = "Annual check-up", Notes = "Patient is in good health. BP normal.", ServiceOfferingId = smithCheckup?.Id });
            appointments.Add(new() { PatientId = patients[2].Id, DoctorId = smithId, StartTime = refMonday.AddHours(10), EndTime = refMonday.AddHours(10.25), Status = AppointmentStatus.Completed, Reason = "Diabetes follow-up", Notes = "Blood sugar levels stable. Continuing metformin.", ServiceOfferingId = smithCheckup?.Id });
            appointments.Add(new() { PatientId = patients[4].Id, DoctorId = smithId, StartTime = refMonday.AddHours(14), EndTime = refMonday.AddHours(14.25), Status = AppointmentStatus.Cancelled, Reason = "Mild fever", CancellationReason = "Patient rescheduled" });

            // Tue 23 Jun – Dr. Chen
            appointments.Add(new() { PatientId = patients[1].Id, DoctorId = chenId, StartTime = refMonday.AddDays(1).AddHours(9), EndTime = refMonday.AddDays(1).AddHours(9.25), Status = AppointmentStatus.Confirmed, Reason = "Cardiology consultation", Notes = "Patient reports occasional chest tightness.", ServiceOfferingId = chenConsult?.Id });
            appointments.Add(new() { PatientId = patients[5].Id, DoctorId = chenId, StartTime = refMonday.AddDays(1).AddHours(10), EndTime = refMonday.AddDays(1).AddHours(10.25), Status = AppointmentStatus.InProgress, Reason = "Hypertension check", Notes = "BP reading: 140/90. Adjusting medication.", ServiceOfferingId = chenConsult?.Id });
            appointments.Add(new() { PatientId = patients[7].Id, DoctorId = chenId, StartTime = refMonday.AddDays(1).AddHours(11), EndTime = refMonday.AddDays(1).AddHours(11.25), Status = AppointmentStatus.Scheduled, Reason = "Pre-surgery evaluation" });

            // Wed 24 Jun – Dr. Smith + Chen
            appointments.Add(new() { PatientId = patients[3].Id, DoctorId = smithId, StartTime = refMonday.AddDays(2).AddHours(9), EndTime = refMonday.AddDays(2).AddHours(9.25), Status = AppointmentStatus.Scheduled, Reason = "General consultation", ServiceOfferingId = smithCheckup?.Id });
            appointments.Add(new() { PatientId = patients[8].Id, DoctorId = smithId, StartTime = refMonday.AddDays(2).AddHours(10), EndTime = refMonday.AddDays(2).AddHours(10.25), Status = AppointmentStatus.Scheduled, Reason = "Asthma follow-up", Notes = "Bring peak flow meter readings.", ServiceOfferingId = smithCheckup?.Id });
            appointments.Add(new() { PatientId = patients[6].Id, DoctorId = chenId, StartTime = refMonday.AddDays(2).AddHours(14), EndTime = refMonday.AddDays(2).AddHours(14.25), Status = AppointmentStatus.Scheduled, Reason = "ECG test follow-up", ServiceOfferingId = chenECG?.Id });

            // Thu 25 Jun
            appointments.Add(new() { PatientId = patients[9].Id, DoctorId = chenId, StartTime = refMonday.AddDays(3).AddHours(9), EndTime = refMonday.AddDays(3).AddHours(9.25), Status = AppointmentStatus.Scheduled, Reason = "Annual cardiac assessment", ServiceOfferingId = chenConsult?.Id });
            appointments.Add(new() { PatientId = patients[10].Id, DoctorId = smithId, StartTime = refMonday.AddDays(3).AddHours(15), EndTime = refMonday.AddDays(3).AddHours(15.25), Status = AppointmentStatus.Scheduled, Reason = "Company medical exam", ServiceOfferingId = smithCheckup?.Id });

            // Fri 26 Jun
            appointments.Add(new() { PatientId = patients[11].Id, DoctorId = smithId, StartTime = refMonday.AddDays(4).AddHours(10), EndTime = refMonday.AddDays(4).AddHours(10.25), Status = AppointmentStatus.Scheduled, Reason = "Skin rash consultation" });
            // Extra June: Dr. Chen Fri 26
            appointments.Add(new() { PatientId = patients[3].Id, DoctorId = chenId, StartTime = refMonday.AddDays(4).AddHours(14), EndTime = refMonday.AddDays(4).AddHours(14.25), Status = AppointmentStatus.Cancelled, Reason = "Heart palpitations follow-up", CancellationReason = "Patient feeling better, cancelled" });

            // ── JULY 2026 — busy calendar ──────────────────────────────
            // Wed 1 Jul — Fri 31 Jul: ~2-3 appts per doctor per weekday

            var julyDates = new List<DateTime>();
            for (var d = july1; d <= new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Utc); d = d.AddDays(1))
                if (d.DayOfWeek >= DayOfWeek.Monday && d.DayOfWeek <= DayOfWeek.Friday)
                    julyDates.Add(d);

            // We need predictable status rotation across the month
            var statusPool = new[] {
                AppointmentStatus.Scheduled, AppointmentStatus.Confirmed, AppointmentStatus.InProgress,
                AppointmentStatus.Completed, AppointmentStatus.Completed, AppointmentStatus.Completed,
                AppointmentStatus.Scheduled, AppointmentStatus.Confirmed, AppointmentStatus.Cancelled,
                AppointmentStatus.Completed, AppointmentStatus.Completed, AppointmentStatus.NoShow
            };

            var reasonsSmith = new[] {
                "General check-up", "BP monitoring", "Diabetes follow-up", "Vaccination", "Cough and colds",
                "Annual physical", "Medication review", "ECG check", "Wound care follow-up", "Weight management"
            };
            var reasonsChen = new[] {
                "Cardiac consultation", "ECG follow-up", "Blood pressure check", "Chest pain evaluation",
                "Stress test review", "Holter monitor results", "Echo follow-up", "Pre-op cardiac clearance",
                "Hypertension management", "Lipid profile review"
            };

            var patientIdx = 0;
            foreach (var date in julyDates)
            {
                var dayStatus = statusPool[patientIdx % statusPool.Length];
                var dayStatus2 = statusPool[(patientIdx + 3) % statusPool.Length];
                var dayStatus3 = statusPool[(patientIdx + 7) % statusPool.Length];

                var p1 = patients[patientIdx % patients.Count].Id;
                var p2 = patients[(patientIdx + 2) % patients.Count].Id;
                var p3 = patients[(patientIdx + 5) % patients.Count].Id;
                var p4 = patients[(patientIdx + 8) % patients.Count].Id;

                var r1 = reasonsSmith[patientIdx % reasonsSmith.Length];
                var r2 = reasonsSmith[(patientIdx + 3) % reasonsSmith.Length];
                var r3 = reasonsChen[patientIdx % reasonsChen.Length];
                var r4 = reasonsChen[(patientIdx + 4) % reasonsChen.Length];

                // Dr. Smith: 2-3 per day at 9am, 11am, 2pm
                appointments.Add(new() { PatientId = p1, DoctorId = smithId, StartTime = date.AddHours(9), EndTime = date.AddHours(9.25), Status = dayStatus, Reason = r1 });
                appointments.Add(new() { PatientId = p2, DoctorId = smithId, StartTime = date.AddHours(11), EndTime = date.AddHours(11.25), Status = dayStatus2, Reason = r2 });
                if (patientIdx % 3 != 0) // skip occasional slot
                    appointments.Add(new() { PatientId = p3, DoctorId = smithId, StartTime = date.AddHours(14), EndTime = date.AddHours(14.25), Status = dayStatus3, Reason = reasonsSmith[(patientIdx + 6) % reasonsSmith.Length] });

                // Dr. Chen: 2-3 per day at 10am, 1pm, 3pm
                appointments.Add(new() { PatientId = p4, DoctorId = chenId, StartTime = date.AddHours(10), EndTime = date.AddHours(10.25), Status = dayStatus, Reason = r3 });
                appointments.Add(new() { PatientId = p1, DoctorId = chenId, StartTime = date.AddHours(13), EndTime = date.AddHours(13.25), Status = dayStatus2, Reason = r4 });
                if (patientIdx % 2 != 0)
                    appointments.Add(new() { PatientId = p2, DoctorId = chenId, StartTime = date.AddHours(15), EndTime = date.AddHours(15.25), Status = dayStatus3, Reason = reasonsChen[(patientIdx + 7) % reasonsChen.Length] });

                patientIdx++;
            }

            context.Appointments.AddRange(appointments);
            await context.SaveChangesAsync();
        }

        // ── MEDICAL RECORDS ──────────────────────────────────────────────────
        // 30 records linked to Completed / InProgress appointments

        if (!await context.MedicalRecords.AnyAsync())
        {
            var smithDoctor = await context.Doctors.FirstAsync(d => d.LicenseNumber == "LIC-001");
            var chenDoctor = await context.Doctors.FirstAsync(d => d.LicenseNumber == "LIC-002");
            var completedAppts = await context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.InProgress)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
            var meds = await context.Medicines.ToListAsync();

            var paracetamol = meds.First(m => m.Name == "Paracetamol");
            var metformin = meds.First(m => m.Name == "Metformin");
            var amoxicillin = meds.First(m => m.Name == "Amoxicillin");
            var losartan = meds.First(m => m.Name == "Losartan");
            var atorvastatin = meds.First(m => m.Name == "Atorvastatin");
            var cetirizine = meds.First(m => m.Name == "Cetirizine");

            var diagnoses = new[] {
                "Essential hypertension", "Type 2 diabetes mellitus, stable", "Acute upper respiratory tract infection",
                "Allergic rhinitis", "Gastroesophageal reflux disease (GERD)", "Hyperlipidemia",
                "Bronchial asthma, mild persistent", "Urinary tract infection", "Osteoarthritis, knee",
                "Iron deficiency anemia", "Hypothyroidism", "Acute gastroenteritis",
                "Tension-type headache", "Insomnia, chronic", "Dengue fever, recovered",
                "Mild dehydration", "Cellulitis, lower extremity", "Chronic kidney disease stage 2",
                "Atopic dermatitis", "Acute sinusitis", "Plantar fasciitis", "Anxiety disorder, mild",
                "Vitamin D deficiency", "Migraine without aura", "Benign prostatic hyperplasia",
                "Dyslipidemia with mixed pattern", "Chronic obstructive pulmonary disease, stable",
                "Peptic ulcer disease", "Gout, acute flare", "Epigastric pain, unspecified"
            };

            var recordStatuses = new[] {
                true, true, true, false, true, true, false, true, true, true,
                false, true, true, true, true, false, true, false, true, true,
                true, false, true, true, true, true, true, true, true, false
            };

            var records = new List<MedicalRecord>();
            for (int i = 0; i < Math.Min(completedAppts.Count, 30); i++)
            {
                var appt = completedAppts[i];
                var isSmith = appt.DoctorId == smithDoctor.Id;
                var docId = isSmith ? smithDoctor.Id : chenDoctor.Id;
                var isCompletable = appt.Status == AppointmentStatus.Completed;

                var record = new MedicalRecord
                {
                    PatientId = appt.PatientId,
                    DoctorId = docId,
                    AppointmentId = appt.Id,
                    Diagnosis = diagnoses[i % diagnoses.Length],
                    Symptoms = GenerateSymptoms(diagnoses[i % diagnoses.Length]),
                    Treatment = GenerateTreatment(diagnoses[i % diagnoses.Length]),
                    Notes = $"Follow-up recommended in {(i % 3 == 0 ? "3 months" : i % 3 == 1 ? "1 month" : "6 months")}.",
                    IsConfidential = i % 5 == 4,
                    IsCompleted = isCompletable,
                    CreatedAt = appt.StartTime,
                    UpdatedAt = appt.StartTime,
                };

                if (i % 2 == 0)
                {
                    var medIdx = i % 5;
                    var statusIdx = i % 3;
                    Medicine med = medIdx switch
                    {
                        0 => metformin,
                        1 => losartan,
                        2 => paracetamol,
                        3 => atorvastatin,
                        _ => cetirizine,
                    };
                    PrescriptionStatus prescStatus = statusIdx switch
                    {
                        0 => PrescriptionStatus.Pending,
                        1 => PrescriptionStatus.Paid,
                        _ => PrescriptionStatus.Completed,
                    };
                    var dosage = med == metformin ? "500mg" : med == losartan ? "50mg" : med == atorvastatin ? "20mg" : "1 tablet";
                    record.Prescriptions.Add(new Prescription
                    {
                        MedicineId = med.Id,
                        Dosage = dosage,
                        Frequency = i % 2 == 0 ? "Once daily" : "Twice daily",
                        Duration = $"{30 + (i % 3) * 30} days",
                        Quantity = 30 + (i % 4) * 15,
                        Status = prescStatus,
                    });
                }

                records.Add(record);
            }

            context.MedicalRecords.AddRange(records);
            await context.SaveChangesAsync();
        }

        // ── LAB ORDERS ──────────────────────────────────────────────────────
        // 30 lab orders with varied statuses

        if (!await context.LabOrders.AnyAsync())
        {
            var doctors = await context.Doctors.ToListAsync();
            var patients = await context.Patients.ToListAsync();
            var labTests = await context.LabTests.Where(t => t.IsActive).ToListAsync();
            var smithId = doctors.First(d => d.LicenseNumber == "LIC-001").Id;
            var chenId = doctors.First(d => d.LicenseNumber == "LIC-002").Id;

            var labResults = new Dictionary<string, (string Result, string Range)>
            {
                ["Complete Blood Count (CBC)"] = ("WBC 6.5, RBC 4.8, Hgb 14.2, Hct 42%, Plt 250", "WBC 4.5-11.0, RBC 4.2-5.9, Hgb 13.5-17.5, Plt 150-450"),
                ["Fasting Blood Sugar"] = ("5.2 mmol/L", "3.9-5.6 mmol/L"),
                ["HbA1c"] = ("6.5%", "< 7.0%"),
                ["Lipid Panel"] = ("TC 5.8, LDL 3.5, HDL 1.2, TG 2.1", "TC < 5.2, LDL < 3.4, HDL > 1.0, TG < 1.7"),
                ["Creatinine"] = ("85 umol/L", "60-110 umol/L"),
                ["BUN"] = ("6.2 mmol/L", "3.2-7.3 mmol/L"),
                ["ALT / SGPT"] = ("35 U/L", "10-40 U/L"),
                ["TSH"] = ("2.8 mIU/L", "0.4-4.0 mIU/L"),
                ["Vitamin D (25-OH)"] = ("75 nmol/L", "75-250 nmol/L"),
            };

            var statuses = new[] {
                LabOrderStatus.Ordered, LabOrderStatus.Ordered, LabOrderStatus.Collected,
                LabOrderStatus.Collected, LabOrderStatus.Processing, LabOrderStatus.Processing,
                LabOrderStatus.Completed, LabOrderStatus.Completed, LabOrderStatus.Completed,
                LabOrderStatus.Cancelled
            };

            var orders = new List<LabOrder>();
            var now = DateTime.UtcNow;

            for (int i = 0; i < 30; i++)
            {
                var p = patients[i % patients.Count];
                var docId = i % 2 == 0 ? smithId : chenId;
                var test = labTests[i % labTests.Count];
                var status = statuses[i % statuses.Length];
                var isCompleted = status == LabOrderStatus.Completed;

                var resultKey = labResults.Keys.FirstOrDefault(k => test.TestName.Contains(k.Split('(')[0].Trim())
                    || k.Contains(test.TestName.Split('(')[0].Trim()));
                var hasResult = isCompleted && resultKey != null;

                orders.Add(new LabOrder
                {
                    PatientId = p.Id,
                    DoctorId = docId,
                    LabTestId = test.Id,
                    Status = status,
                    Result = hasResult ? labResults[resultKey!].Result : null,
                    ReferenceRange = hasResult ? labResults[resultKey!].Range : null,
                    ResultSummary = hasResult ? (labResults[resultKey!].Result.Contains("normal", StringComparison.OrdinalIgnoreCase) || labResults[resultKey!].Result.Contains("5.2") ? "Normal range" : "Borderline — follow-up recommended") : null,
                    Notes = i % 3 == 0 ? "Fasting required" : i % 3 == 1 ? "Patient instructed to avoid caffeine" : null,
                    CompletedAt = isCompleted ? now.AddDays(-(30 - i)) : null,
                    OrderedAt = now.AddDays(-(45 - i)),
                });
            }

            context.LabOrders.AddRange(orders);
            await context.SaveChangesAsync();
        }

        // ── BILLINGS ───────────────────────────────────────────────────────
        // 30 billings with varied statuses + items

        if (!await context.Billings.AnyAsync())
        {
            var adminUser = await context.Users.FirstAsync(u => u.UserName == "admin");
            var patients = await context.Patients.ToListAsync();
            var billableAppts = await context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.InProgress)
                .OrderBy(a => a.StartTime)
                .Take(28)
                .ToListAsync();

            var billings = new List<Billing>();
            var invoiceCounter = 1;

            // First 5 billings match the original seed structure
            if (billableAppts.Count > 0)
            {
                var total0 = 500 + 50;
                billings.Add(new() { PatientId = billableAppts[0].PatientId, AppointmentId = billableAppts[0].Id, InvoiceNumber = $"INV-2026-{invoiceCounter++:D4}", SubTotal = 500, Discount = 0, Tax = 50, Total = total0, AmountPaid = total0, Status = BillingStatus.Paid, DueDate = today.AddDays(7), CreatedById = adminUser.Id, Notes = "Annual check-up consultation fee" });
            }
            if (billableAppts.Count > 1)
            {
                var total1 = 500 + 50;
                billings.Add(new() { PatientId = billableAppts[1].PatientId, AppointmentId = billableAppts[1].Id, InvoiceNumber = $"INV-2026-{invoiceCounter++:D4}", SubTotal = 500, Discount = 0, Tax = 50, Total = total1, AmountPaid = 0, Status = BillingStatus.Pending, DueDate = today.AddDays(14), CreatedById = adminUser.Id, Notes = "Diabetes follow-up consultation" });
            }
            if (billableAppts.Count > 2)
            {
                var sub3 = 800;
                var total3 = sub3 - 100 + 70;
                billings.Add(new() { PatientId = billableAppts[2].PatientId, AppointmentId = billableAppts[2].Id, InvoiceNumber = $"INV-2026-{invoiceCounter++:D4}", SubTotal = sub3, Discount = 100, Tax = 70, Total = total3, AmountPaid = 400, Status = BillingStatus.PartiallyPaid, DueDate = today.AddDays(7), CreatedById = adminUser.Id, Notes = "Cardiology consultation (with senior discount)" });
            }
            if (billableAppts.Count > 3)
            {
                var sub4 = 800 + 200;
                var total4 = sub4 + 80;
                billings.Add(new() { PatientId = billableAppts[3].PatientId, AppointmentId = billableAppts[3].Id, InvoiceNumber = $"INV-2026-{invoiceCounter++:D4}", SubTotal = sub4, Discount = 0, Tax = 80, Total = total4, AmountPaid = 0, Status = BillingStatus.Pending, DueDate = today.AddDays(10), CreatedById = adminUser.Id, Notes = "Hypertension check + lab fees" });
            }
            // Standalone cancelled billing
            billings.Add(new() { PatientId = patients[0].Id, AppointmentId = null, InvoiceNumber = $"INV-2026-{invoiceCounter++:D4}", SubTotal = 250, Discount = 0, Tax = 25, Total = 275, AmountPaid = 0, Status = BillingStatus.Cancelled, DueDate = today.AddDays(-5), CreatedById = adminUser.Id, Notes = "Lab work order (cancelled)" });

            // Remaining 25 billings
            var billingStatusPool = new[] {
                BillingStatus.Pending, BillingStatus.Paid, BillingStatus.PartiallyPaid,
                BillingStatus.Pending, BillingStatus.Paid, BillingStatus.Cancelled,
                BillingStatus.Paid, BillingStatus.Pending, BillingStatus.PartiallyPaid,
                BillingStatus.Refunded, BillingStatus.Paid, BillingStatus.Pending,
                BillingStatus.PartiallyPaid, BillingStatus.Paid, BillingStatus.Pending,
                BillingStatus.Paid, BillingStatus.Pending, BillingStatus.PartiallyPaid,
                BillingStatus.Cancelled, BillingStatus.Paid, BillingStatus.Pending,
                BillingStatus.Paid, BillingStatus.Refunded, BillingStatus.Pending, BillingStatus.Paid
            };

            var serviceNames = new[] {
                "General Check-up", "ECG", "Cardiac Consultation", "Lipid Panel", "Fasting Blood Sugar",
                "Stress Test", "HbA1c", "Urinalysis", "Echocardiogram", "CBC"
            };
            var servicePrices = new[] { 500m, 350m, 800m, 800m, 150m, 1500m, 600m, 200m, 2500m, 350m };

            for (int i = 5; i < 30; i++)
            {
                var pi = (i - 5) % patients.Count;
                var patId = patients[pi].Id;
                var apptId = (i - 5) < billableAppts.Count ? billableAppts[i - 5].Id : (Guid?)null;
                var status = billingStatusPool[(i - 5) % billingStatusPool.Length];
                var isPaid = status == BillingStatus.Paid || status == BillingStatus.Refunded;
                var isCancelled = status == BillingStatus.Cancelled || status == BillingStatus.Refunded;

                var svcIdx = (i - 5) % serviceNames.Length;
                var desc = $"{serviceNames[svcIdx]} — Consultation";
                var price = servicePrices[svcIdx];
                var qty = 1;
                var subTotal = price;
                var tax = subTotal * 0.10m;
                var discount = i % 4 == 0 ? 100m : 0m;
                var total = subTotal - discount + tax;
                var amountPaid = isPaid ? total : status == BillingStatus.PartiallyPaid ? Math.Round(total / 2) : 0m;

                var billing = new Billing
                {
                    PatientId = patId,
                    AppointmentId = apptId,
                    InvoiceNumber = $"INV-2026-{invoiceCounter++:D4}",
                    SubTotal = subTotal,
                    Discount = discount,
                    Tax = tax,
                    Total = total,
                    AmountPaid = amountPaid,
                    Status = status,
                    DueDate = today.AddDays(isCancelled ? -5 : 7 + (i % 3) * 7),
                    CreatedById = adminUser.Id,
                    Notes = $"{desc} — {(isPaid ? "Fully paid" : isCancelled ? "Cancelled" : status == BillingStatus.PartiallyPaid ? "Partial payment received" : "Awaiting payment")}",
                };

                billing.Items.Add(new BillingItem { Description = desc, Quantity = qty, UnitPrice = price, Total = total });
                billings.Add(billing);
            }

            context.Billings.AddRange(billings);
            await context.SaveChangesAsync();
        }

        // ── PAYMENTS ────────────────────────────────────────────────────────
        // 10 payments for Paid / PartiallyPaid billings

        if (!await context.Payments.AnyAsync())
        {
            var adminUser = await context.Users.FirstAsync(u => u.UserName == "admin");
            var paidBillings = await context.Billings
                .Where(b => b.Status == BillingStatus.Paid || b.Status == BillingStatus.PartiallyPaid)
                .Take(10)
                .ToListAsync();

            var methodSource = new[] { PaymentMethod.Cash, PaymentMethod.Card, PaymentMethod.Online, PaymentMethod.Insurance, PaymentMethod.Cash };
            var refPrefix = new[] { "CASH", "CARD", "ONLN", "INSU", "CASH" };

            var payments = new List<Payment>();
            for (int i = 0; i < paidBillings.Count; i++)
            {
                var billing = paidBillings[i];
                var method = methodSource[i % methodSource.Length];

                payments.Add(new Payment
                {
                    BillingId = billing.Id,
                    Amount = billing.AmountPaid > 0 ? billing.AmountPaid : billing.Total,
                    PaymentMethod = method,
                    TransactionReference = method == PaymentMethod.Cash ? null : $"{refPrefix[i % refPrefix.Length]}-{DateTime.UtcNow:yyyyMMdd}-{i + 1:D4}",
                    IsVerified = true,
                    ReceivedById = adminUser.Id,
                    Notes = method == PaymentMethod.Insurance ? "Claim submitted to Maxicare" : null,
                });
            }

            context.Payments.AddRange(payments);
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();
    }

    private static string GenerateSymptoms(string diagnosis) => diagnosis switch
    {
        string d when d.Contains("hypertension", StringComparison.OrdinalIgnoreCase) => "Occasional headache, mild dizziness. Asymptomatic at rest.",
        string d when d.Contains("diabetes", StringComparison.OrdinalIgnoreCase) => "Mild fatigue, polydipsia, polyuria. Recent HbA1c elevated.",
        string d when d.Contains("respiratory", StringComparison.OrdinalIgnoreCase) => "Cough with phlegm, slight fever 37.8°C, nasal congestion for 3 days.",
        string d when d.Contains("rhinitis", StringComparison.OrdinalIgnoreCase) => "Sneezing, runny nose, itchy eyes. Symptoms worse in mornings.",
        string d when d.Contains("GERD", StringComparison.OrdinalIgnoreCase) => "Heartburn after meals, regurgitation, occasional dysphagia.",
        string d when d.Contains("lipid", StringComparison.OrdinalIgnoreCase) => "Asymptomatic. Elevated cholesterol on routine labs.",
        string d when d.Contains("asthma", StringComparison.OrdinalIgnoreCase) => "Wheezing, shortness of breath on exertion, nocturnal cough 2x/week.",
        string d when d.Contains("urinary", StringComparison.OrdinalIgnoreCase) => "Dysuria, urinary frequency, suprapubic discomfort.",
        string d when d.Contains("osteoarthritis", StringComparison.OrdinalIgnoreCase) => "Knee pain on weight-bearing, stiffness in morning, crepitus.",
        string d when d.Contains("anemia", StringComparison.OrdinalIgnoreCase) => "Fatigue, pallor, shortness of breath on mild exertion.",
        string d when d.Contains("thyroid", StringComparison.OrdinalIgnoreCase) => "Weight gain, fatigue, cold intolerance, dry skin.",
        string d when d.Contains("gastroenteritis", StringComparison.OrdinalIgnoreCase) => "Watery diarrhea 5x/day, nausea, mild abdominal cramps.",
        string d when d.Contains("headache", StringComparison.OrdinalIgnoreCase) => "Bilateral pressing headache, non-throbbing, lasting hours.",
        string d when d.Contains("insomnia", StringComparison.OrdinalIgnoreCase) => "Difficulty falling asleep, frequent waking, daytime fatigue.",
        string d when d.Contains("dengue", StringComparison.OrdinalIgnoreCase) => "Fever 39.2°C, body aches, retro-orbital pain, petechiae on arms.",
        string d when d.Contains("dehydration", StringComparison.OrdinalIgnoreCase) => "Thirst, dry mucous membranes, decreased urine output, dizziness.",
        string d when d.Contains("cellulitis", StringComparison.OrdinalIgnoreCase) => "Redness, swelling, warmth on left lower leg. Mild pain.",
        string d when d.Contains("kidney", StringComparison.OrdinalIgnoreCase) => "Mild pedal edema. Elevated creatinine on recent labs.",
        string d when d.Contains("dermatitis", StringComparison.OrdinalIgnoreCase) => "Dry, itchy patches on flexural areas. Flaring up 2 weeks.",
        string d when d.Contains("sinusitis", StringComparison.OrdinalIgnoreCase) => "Facial pain over maxillary sinuses, purulent nasal discharge.",
        string d when d.Contains("fasciitis", StringComparison.OrdinalIgnoreCase) => "Sharp heel pain on first steps in morning, improves during day.",
        string d when d.Contains("anxiety", StringComparison.OrdinalIgnoreCase) => "Excessive worry, restlessness, difficulty concentrating, palpitations.",
        string d when d.Contains("vitamin", StringComparison.OrdinalIgnoreCase) => "Generalized bone pain, muscle weakness, fatigue.",
        string d when d.Contains("migraine", StringComparison.OrdinalIgnoreCase) => "Unilateral throbbing headache with photophobia and nausea.",
        string d when d.Contains("prostatic", StringComparison.OrdinalIgnoreCase) => "Hesitancy, weak stream, nocturia 3-4x.",
        string d when d.Contains("dyslipidemia", StringComparison.OrdinalIgnoreCase) => "Asymptomatic. Mixed hyperlipidemia on recent fasting panel.",
        string d when d.Contains("pulmonary", StringComparison.OrdinalIgnoreCase) => "Chronic cough, sputum production, dyspnea on exertion.",
        string d when d.Contains("peptic", StringComparison.OrdinalIgnoreCase) => "Epigastric burning pain 2hrs after meals, relieved by antacids.",
        string d when d.Contains("gout", StringComparison.OrdinalIgnoreCase) => "Acute pain, swelling, redness in right great toe. Unable to walk.",
        _ => "Patient reports general discomfort. Vitals stable."
    };

    private static string GenerateTreatment(string diagnosis) => diagnosis switch
    {
        string d when d.Contains("hypertension", StringComparison.OrdinalIgnoreCase) => "Start Losartan 50mg once daily. Low-sodium diet, regular exercise. Follow-up in 1 month.",
        string d when d.Contains("diabetes", StringComparison.OrdinalIgnoreCase) => "Continue Metformin 500mg BID. Dietician referral. HbA1c target < 7.0%.",
        string d when d.Contains("respiratory", StringComparison.OrdinalIgnoreCase) => "Amoxicillin 500mg TID x 7 days. Paracetamol PRN for fever. Increase fluid intake.",
        string d when d.Contains("rhinitis", StringComparison.OrdinalIgnoreCase) => "Cetirizine 10mg once daily. Saline nasal spray. Avoid known allergens.",
        string d when d.Contains("GERD", StringComparison.OrdinalIgnoreCase) => "Omeprazole 20mg before breakfast. Elevate head of bed. Avoid trigger foods.",
        string d when d.Contains("lipid", StringComparison.OrdinalIgnoreCase) => "Atorvastatin 20mg once daily at bedtime. Dietary modifications. Repeat lipid panel in 3 months.",
        string d when d.Contains("asthma", StringComparison.OrdinalIgnoreCase) => "Salbutamol inhaler 2 puffs PRN. Fluticasone inhaler 1 puff BID. Asthma action plan provided.",
        string d when d.Contains("urinary", StringComparison.OrdinalIgnoreCase) => "Nitrofurantoin 100mg BID x 5 days. Increased fluid intake. Urine culture if no improvement.",
        string d when d.Contains("osteoarthritis", StringComparison.OrdinalIgnoreCase) => "Paracetamol 500mg PRN up to TID. Physical therapy referral. Weight management counseling.",
        string d when d.Contains("anemia", StringComparison.OrdinalIgnoreCase) => "Ferrous sulfate 325mg once daily. Dietary iron sources advised. Repeat CBC in 2 months.",
        string d when d.Contains("thyroid", StringComparison.OrdinalIgnoreCase) => "Levothyroxine 50mcg once daily on empty stomach. Repeat TSH in 6 weeks.",
        string d when d.Contains("gastroenteritis", StringComparison.OrdinalIgnoreCase) => "Oral rehydration salts. Paracetamol for fever. BRAT diet. Return if persistent > 48 hours.",
        string d when d.Contains("headache", StringComparison.OrdinalIgnoreCase) => "Paracetamol/caffeine combination PRN. Stress reduction. Sleep hygiene counseling.",
        string d when d.Contains("insomnia", StringComparison.OrdinalIgnoreCase) => "Sleep hygiene education. Melatonin 5mg at bedtime. Avoid caffeine after 2pm.",
        string d when d.Contains("dengue", StringComparison.OrdinalIgnoreCase) => "Strict bed rest. Paracetamol for fever (avoid NSAIDs). Monitor platelet count daily. IV fluids if needed.",
        string d when d.Contains("dehydration", StringComparison.OrdinalIgnoreCase) => "Oral rehydration therapy. Paracetamol PRN for fever. Monitor urine output.",
        string d when d.Contains("cellulitis", StringComparison.OrdinalIgnoreCase) => "Co-Amoxiclav 625mg TID x 7 days. Elevate affected limb. Warm compress TID.",
        string d when d.Contains("kidney", StringComparison.OrdinalIgnoreCase) => "Referral to nephrology. BP control target < 130/80. Low-protein diet. Avoid NSAIDs.",
        string d when d.Contains("dermatitis", StringComparison.OrdinalIgnoreCase) => "Topical hydrocortisone 1% cream BID x 7 days. Emollient moisturizer. Avoid harsh soaps.",
        string d when d.Contains("sinusitis", StringComparison.OrdinalIgnoreCase) => "Amoxicillin 500mg TID x 10 days. Saline nasal irrigation. Decongestant PRN.",
        string d when d.Contains("fasciitis", StringComparison.OrdinalIgnoreCase) => "Stretching exercises. Ice massage. Orthotic inserts. Paracetamol PRN.",
        string d when d.Contains("anxiety", StringComparison.OrdinalIgnoreCase) => "Counseling referral. Breathing exercises. Escitalopram 5mg once daily, titrate up.",
        string d when d.Contains("vitamin", StringComparison.OrdinalIgnoreCase) => "Vitamin D3 2000 IU once daily. Sunlight exposure 15 min/day. Repeat level in 3 months.",
        string d when d.Contains("migraine", StringComparison.OrdinalIgnoreCase) => "Sumatriptan 50mg at onset. Avoid triggers. Sleep hygiene. Consider prophylaxis if frequent.",
        string d when d.Contains("prostatic", StringComparison.OrdinalIgnoreCase) => "Tamsulosin 0.4mg once daily. Avoid caffeine before bed. Review in 1 month.",
        string d when d.Contains("dyslipidemia", StringComparison.OrdinalIgnoreCase) => "Atorvastatin 20mg at bedtime. Omega-3 supplements. Lifestyle modification counseling.",
        string d when d.Contains("pulmonary", StringComparison.OrdinalIgnoreCase) => "Tiotropium inhaler once daily. Pulmonary rehab referral. Smoking cessation counseling.",
        string d when d.Contains("peptic", StringComparison.OrdinalIgnoreCase) => "Omeprazole 20mg once daily. H. pylori stool antigen test. Avoid NSAIDs and alcohol.",
        string d when d.Contains("gout", StringComparison.OrdinalIgnoreCase) => "Colchicine 0.6mg BID x 3 days. Naproxen 500mg BID PRN. Allopurinol start after flare resolves.",
        _ => "Symptomatic treatment. Rest and hydration. Follow-up PRN."
    };
}
