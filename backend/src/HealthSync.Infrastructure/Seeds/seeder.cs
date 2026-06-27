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

        // Admin
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

        // Doctor – John Smith
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

        // Receptionist
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

        // Pharmacist
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

        // Doctor – Emily Chen
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

        // ── APPOINTMENTS ───────────────────────────────────────────────────

        if (!await context.Appointments.AnyAsync())
        {
            var doctors = await context.Doctors.ToListAsync();
            var patients = await context.Patients.ToListAsync();

            var smithId = doctors.First(d => d.LicenseNumber == "LIC-001").Id;
            var chenId = doctors.First(d => d.LicenseNumber == "LIC-002").Id;

            // Use a fixed reference week so seed data is stable
            var refMonday = new DateTime(2026, 6, 22, 0, 0, 0, DateTimeKind.Utc);

            var appointments = new List<Appointment>
            {
                // Mon 22 Jun – Dr. Smith with various patients
                new() {
                    PatientId = patients[0].Id, DoctorId = smithId,
                    StartTime = refMonday.AddHours(9), EndTime = refMonday.AddHours(9.25),
                    Status = AppointmentStatus.Completed, Reason = "Annual check-up",
                    Notes = "Patient is in good health. BP normal."
                },
                new() {
                    PatientId = patients[2].Id, DoctorId = smithId,
                    StartTime = refMonday.AddHours(10), EndTime = refMonday.AddHours(10.25),
                    Status = AppointmentStatus.Completed, Reason = "Diabetes follow-up",
                    Notes = "Blood sugar levels stable. Continuing metformin."
                },
                new() {
                    PatientId = patients[4].Id, DoctorId = smithId,
                    StartTime = refMonday.AddHours(14), EndTime = refMonday.AddHours(14.25),
                    Status = AppointmentStatus.Cancelled, Reason = "Mild fever",
                    CancellationReason = "Patient rescheduled"
                },

                // Tue 23 Jun – Today – Dr. Chen
                new() {
                    PatientId = patients[1].Id, DoctorId = chenId,
                    StartTime = refMonday.AddDays(1).AddHours(9), EndTime = refMonday.AddDays(1).AddHours(9.25),
                    Status = AppointmentStatus.Confirmed, Reason = "Cardiology consultation",
                    Notes = "Patient reports occasional chest tightness."
                },
                new() {
                    PatientId = patients[5].Id, DoctorId = chenId,
                    StartTime = refMonday.AddDays(1).AddHours(10), EndTime = refMonday.AddDays(1).AddHours(10.25),
                    Status = AppointmentStatus.InProgress, Reason = "Hypertension check",
                    Notes = "BP reading: 140/90. Adjusting medication."
                },
                new() {
                    PatientId = patients[7].Id, DoctorId = chenId,
                    StartTime = refMonday.AddDays(1).AddHours(11), EndTime = refMonday.AddDays(1).AddHours(11.25),
                    Status = AppointmentStatus.Scheduled, Reason = "Pre-surgery evaluation"
                },

                // Wed 24 Jun – Dr. Smith
                new() {
                    PatientId = patients[3].Id, DoctorId = smithId,
                    StartTime = refMonday.AddDays(2).AddHours(9), EndTime = refMonday.AddDays(2).AddHours(9.25),
                    Status = AppointmentStatus.Scheduled, Reason = "General consultation"
                },
                new() {
                    PatientId = patients[8].Id, DoctorId = smithId,
                    StartTime = refMonday.AddDays(2).AddHours(10), EndTime = refMonday.AddDays(2).AddHours(10.25),
                    Status = AppointmentStatus.Scheduled, Reason = "Asthma follow-up",
                    Notes = "Bring peak flow meter readings."
                },
                new() {
                    PatientId = patients[6].Id, DoctorId = chenId,
                    StartTime = refMonday.AddDays(2).AddHours(14), EndTime = refMonday.AddDays(2).AddHours(14.25),
                    Status = AppointmentStatus.Scheduled, Reason = "ECG test follow-up"
                },

                // Thu 25 Jun – Dr. Chen
                new() {
                    PatientId = patients[9].Id, DoctorId = chenId,
                    StartTime = refMonday.AddDays(3).AddHours(9), EndTime = refMonday.AddDays(3).AddHours(9.25),
                    Status = AppointmentStatus.Scheduled, Reason = "Annual cardiac assessment"
                },
                new() {
                    PatientId = patients[10].Id, DoctorId = smithId,
                    StartTime = refMonday.AddDays(3).AddHours(15), EndTime = refMonday.AddDays(3).AddHours(15.25),
                    Status = AppointmentStatus.Scheduled, Reason = "Company medical exam"
                },

                // Fri 26 Jun – Dr. Smith
                new() {
                    PatientId = patients[11].Id, DoctorId = smithId,
                    StartTime = refMonday.AddDays(4).AddHours(10), EndTime = refMonday.AddDays(4).AddHours(10.25),
                    Status = AppointmentStatus.Scheduled, Reason = "Skin rash consultation"
                },
            };

            context.Appointments.AddRange(appointments);
            await context.SaveChangesAsync();
        }

        // ── BILLINGS ───────────────────────────────────────────────────────

        if (!await context.Billings.AnyAsync())
        {
            var adminUser = await context.Users.FirstAsync(u => u.UserName == "admin");
            var appointments = await context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.InProgress)
                .ToListAsync();

            var billings = new List<Billing>
            {
                new()
                {
                    PatientId = appointments[0].PatientId,
                    AppointmentId = appointments[0].Id,
                    InvoiceNumber = "INV-2026-0001",
                    SubTotal = 500, Discount = 0, Tax = 50, Total = 550, AmountPaid = 550,
                    Status = BillingStatus.Paid, DueDate = today.AddDays(7),
                    CreatedById = adminUser.Id,
                    Notes = "Annual check-up consultation fee"
                },
                new()
                {
                    PatientId = appointments[1].PatientId,
                    AppointmentId = appointments[1].Id,
                    InvoiceNumber = "INV-2026-0002",
                    SubTotal = 500, Discount = 0, Tax = 50, Total = 550, AmountPaid = 0,
                    Status = BillingStatus.Pending, DueDate = today.AddDays(14),
                    CreatedById = adminUser.Id,
                    Notes = "Diabetes follow-up consultation"
                },
                new()
                {
                    PatientId = appointments[2].PatientId,
                    AppointmentId = appointments[2].Id,
                    InvoiceNumber = "INV-2026-0003",
                    SubTotal = 800, Discount = 100, Tax = 70, Total = 770, AmountPaid = 400,
                    Status = BillingStatus.PartiallyPaid, DueDate = today.AddDays(7),
                    CreatedById = adminUser.Id,
                    Notes = "Cardiology consultation (with senior discount)"
                },
                new()
                {
                    PatientId = appointments[3].PatientId,
                    AppointmentId = appointments[3].Id,
                    InvoiceNumber = "INV-2026-0004",
                    SubTotal = 800, Discount = 0, Tax = 80, Total = 880, AmountPaid = 0,
                    Status = BillingStatus.Pending, DueDate = today.AddDays(10),
                    CreatedById = adminUser.Id,
                    Notes = "Hypertension check + lab fees"
                },
                new()
                {
                    PatientId = appointments[0].PatientId,
                    AppointmentId = null,
                    InvoiceNumber = "INV-2026-0005",
                    SubTotal = 250, Discount = 0, Tax = 25, Total = 275, AmountPaid = 0,
                    Status = BillingStatus.Cancelled, DueDate = today.AddDays(-5),
                    CreatedById = adminUser.Id,
                    Notes = "Lab work order (cancelled)"
                },
            };

            context.Billings.AddRange(billings);
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
                new Medicine { Name = "Metformin", GenericName = "Metformin HCl", Category = "Antidiabetic", Unit = "tablet", Price = 5.00m, ReorderLevel = 50 }
            );

            await context.SaveChangesAsync();
        }

        // Ensure Metformin exists even if Medicines block was skipped (prior seed)
        if (!await context.Medicines.AnyAsync(m => m.Name == "Metformin"))
        {
            context.Medicines.Add(new Medicine
            {
                Name = "Metformin", GenericName = "Metformin HCl",
                Category = "Antidiabetic", Unit = "tablet",
                Price = 5.00m, ReorderLevel = 50
            });
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

            var batches = new List<InventoryBatch>
            {
                new() { MedicineId = paracetamol.Id, BatchNumber = "PCM-2026-A01", Quantity = 500, UnitPrice = 1.80m, ManufactureDate = new DateOnly(2026, 1, 15), ExpiryDate = new DateOnly(2028, 1, 15), Supplier = "PharmaCorp Inc.", Location = "Shelf A-1" },
                new() { MedicineId = paracetamol.Id, BatchNumber = "PCM-2026-A02", Quantity = 200, UnitPrice = 2.00m, ManufactureDate = new DateOnly(2026, 3, 1), ExpiryDate = new DateOnly(2028, 3, 1), Supplier = "MedSupply Co.", Location = "Shelf A-1" },
                new() { MedicineId = amoxicillin.Id, BatchNumber = "AMX-2026-B01", Quantity = 300, UnitPrice = 5.50m, ManufactureDate = new DateOnly(2026, 2, 10), ExpiryDate = new DateOnly(2027, 8, 10), Supplier = "PharmaCorp Inc.", Location = "Shelf B-2" },
                new() { MedicineId = amoxicillin.Id, BatchNumber = "AMX-2026-B02", Quantity = 150, UnitPrice = 6.00m, ManufactureDate = new DateOnly(2026, 4, 5), ExpiryDate = new DateOnly(2027, 10, 5), Supplier = "HealthPlus Distributors", Location = "Shelf B-2" },
                new() { MedicineId = omeprazole.Id, BatchNumber = "OME-2026-C01", Quantity = 200, UnitPrice = 9.00m, ManufactureDate = new DateOnly(2026, 1, 20), ExpiryDate = new DateOnly(2028, 7, 20), Supplier = "MedSupply Co.", Location = "Shelf C-1" },
                new() { MedicineId = omeprazole.Id, BatchNumber = "OME-2026-C02", Quantity = 80, UnitPrice = 10.00m, ManufactureDate = new DateOnly(2025, 11, 1), ExpiryDate = new DateOnly(2027, 5, 1), Supplier = "PharmaCorp Inc.", Location = "Shelf C-1" },
                new() { MedicineId = salbutamol.Id, BatchNumber = "SAL-2026-D01", Quantity = 30, UnitPrice = 120.00m, ManufactureDate = new DateOnly(2026, 3, 15), ExpiryDate = new DateOnly(2027, 9, 15), Supplier = "RespireMed Ltd.", Location = "Shelf D-1" },
                new() { MedicineId = cetirizine.Id, BatchNumber = "CTZ-2026-E01", Quantity = 400, UnitPrice = 2.00m, ManufactureDate = new DateOnly(2026, 5, 1), ExpiryDate = new DateOnly(2028, 5, 1), Supplier = "HealthPlus Distributors", Location = "Shelf E-1" },
                new() { MedicineId = cetirizine.Id, BatchNumber = "CTZ-2026-E02", Quantity = 250, UnitPrice = 2.20m, ManufactureDate = new DateOnly(2025, 8, 1), ExpiryDate = new DateOnly(2027, 2, 1), Supplier = "PharmaCorp Inc.", Location = "Shelf E-1" },
            };

            context.InventoryBatches.AddRange(batches);
            await context.SaveChangesAsync();
        }

        // ── MEDICAL RECORDS ──────────────────────────────────────────────────

        if (!await context.MedicalRecords.AnyAsync())
        {
            var smithDoctor = await context.Doctors.FirstAsync(d => d.LicenseNumber == "LIC-001");
            var completed = await context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
            var meds = await context.Medicines.ToListAsync();
            var metformin = meds.First(m => m.Name == "Metformin");

            var records = new List<MedicalRecord>
            {
                new()
                {
                    PatientId = completed[0].PatientId,
                    DoctorId = smithDoctor.Id,
                    AppointmentId = completed[0].Id,
                    Diagnosis = "Essential hypertension, benign",
                    Symptoms = "Asymptomatic. Routine annual check-up.",
                    Treatment = "Lifestyle modifications: low-sodium diet, regular aerobic exercise 30 min/day, weight management. Follow-up in 3 months.",
                    Notes = "BP: 125/85. Heart rate: 72 bpm. All lab results within normal range.",
                    IsConfidential = false,
                    CreatedAt = completed[0].StartTime,
                    UpdatedAt = completed[0].StartTime,
                },
                new()
                {
                    PatientId = completed[1].PatientId,
                    DoctorId = smithDoctor.Id,
                    AppointmentId = completed[1].Id,
                    Diagnosis = "Type 2 diabetes mellitus, stable",
                    Symptoms = "Reports mild fatigue in afternoons. Occasional polydipsia.",
                    Treatment = "Continue metformin 500mg twice daily with meals. HbA1c target < 7.0%. Dietician referral provided.",
                    Notes = "Fasting blood glucose: 7.2 mmol/L. HbA1c: 6.8%. Patient adherent to current regimen.",
                    IsConfidential = false,
                    CreatedAt = completed[1].StartTime,
                    UpdatedAt = completed[1].StartTime,
                    Prescriptions =
                    [
                        new()
                        {
                            MedicineId = metformin.Id,
                            Dosage = "500mg",
                            Frequency = "Twice daily",
                            Duration = "90 days",
                            Instructions = "Take with breakfast and dinner. Do not skip meals.",
                            Quantity = 180,
                            Status = PrescriptionStatus.Pending,
                        }
                    ]
                }
            };

            context.MedicalRecords.AddRange(records);
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();
    }
}
