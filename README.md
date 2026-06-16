# HealthSync — Healthcare Resource Management System

A full-stack clinic management platform built with Clean Architecture. Handles patient registration, appointment scheduling (15-min slots), electronic medical records, pharmacy inventory, billing, and role-based access — all with real-time updates.

## Tech Stack

| Layer | Technology |
|---|---|
| **Backend** | C# .NET 9, ASP.NET Core Web API |
| **Architecture** | Clean Architecture (Api / Core / Infrastructure) |
| **Database** | PostgreSQL, EF Core (Code-First, Fluent API) |
| **Auth** | JWT Bearer tokens (password hashing via `PasswordHasher<T>`) |
| **Real-time** | SignalR (appointment & notification hubs) |
| **Frontend** | React 18, TypeScript, Vite |
| **Package Manager** | Bun |
| **Containers** | Docker Compose (PostgreSQL 15, API, Frontend) |

## Features

- **Role-based access** — Admin, Doctor, Receptionist, Pharmacist with granular permissions
- **Appointment scheduling** — 15-minute slots, availability rules, time-off blocking, status lifecycle (Scheduled → Confirmed → InProgress → Completed / Cancelled / NoShow)
- **Patient management** — Full CRUD with search, medical history, allergies, emergency contacts
- **Electronic Medical Records** — Diagnoses, prescriptions, treatment plans, confidentiality flags
- **Pharmacy inventory** — Batch tracking with expiry dates, stock movements (stock-in, dispensing, adjustments), reorder alerts
- **Billing & payments** — Invoicing with line items, multi-payment support (Cash/Card/Online/Insurance), partial payments
- **Reporting** — Aggregated analytics across appointments, revenue, inventory
- **Real-time updates** — Live appointment changes and push notifications via SignalR
- **Audit logging** — All create/update/delete operations tracked with JSON snapshots
- **Docker support** — One-command deployment, no local tool conflicts

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│                    HealthSync.Api                         │
│   Controllers  │  Hubs (SignalR)  │  Middleware  │ JWT   │
│                │                  │              │ Auth  │
└──────────────────────────┬───────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────┐
│                   HealthSync.Core                         │
│   Entities  │  Enums  │  DTOs  │  Interfaces  │  Services │
│   (Domain & Business Logic — no external dependencies)   │
└──────────────────────────┬───────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────┐
│               HealthSync.Infrastructure                    │
│   DbContext  │  Repositories  │  UnitOfWork  │  Seeder   │
│   (EF Core / PostgreSQL — data access implementation)    │
└──────────────────────────────────────────────────────────┘
```

### Domain Model (14 entities)

```
Patients ──1:M── Appointments ──1:0..1── MedicalRecords ──1:M── Prescriptions
  │                 │                           │
  │                 │                           └── Medicines (catalog)
  │                 │
  │                 └── Billings ──1:M── BillingItems
  │                               ──1:M── Payments
  │
  └── Doctors ──1:M── DoctorAvailabilities
                ──1:M── TimeOffs
                ──1:M── Appointments
                ──1:M── MedicalRecords

Medicines ──1:M── InventoryBatches ──1:M── InventoryTransactions

Users ──1:0..1── Patients
Users ──1:0..1── Doctors
```
*All entities use `Guid` primary keys with `gen_random_uuid()`.*

## Getting Started

### Option A — Docker Compose (recommended)

```bash
git clone <repo-url> healthsync
cd healthsync
docker compose up --build
```

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API (Swagger) | http://localhost:5000/swagger |
| PostgreSQL | `localhost:5433` (user: `admin`, pass: `securepass`, db: `healthsync`) |

### Option B — Manual

Prerequisites: .NET 9 SDK, Bun, PostgreSQL running on port 5432.

```bash
# Backend
cd backend
dotnet restore
dotnet run --project src/HealthSync.Api

# Frontend (separate terminal)
cd frontend
bun install
bun run dev
```

[Full installation guide →](guidedoc/setup/installation-guide.md)

## Demo Credentials

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123` | Admin |
| `dr.smith` | `Doctor@123` | Doctor |
| `reception` | `Recept@123` | Receptionist |
| `pharmacist` | `Pharma@123` | Pharmacist |

## API Overview (10 Controllers)

| Endpoint | Description |
|---|---|
| `POST /api/auth/login` | Authenticate, returns JWT + refresh token |
| `POST /api/auth/register` | Register new user (Admin only) |
| `GET/PUT/DELETE /api/patients` | Patient CRUD |
| `GET/PUT/DELETE /api/doctors` | Doctor CRUD + availability management |
| `GET/POST/PUT /api/appointments` | Appointment scheduling + calendar |
| `GET/POST /api/medical-records` | Medical records + prescriptions |
| `GET/POST /api/billings` | Invoices + payments |
| `GET/POST /api/medicines` | Medicine catalog |
| `GET/POST /api/inventory` | Stock batches + transactions |
| `GET /api/reports` | Aggregated analytics |

Full API documentation available at `/swagger` when the backend is running.

## Project Structure

```
backend/
├── src/
│   ├── HealthSync.Api/               # Controllers, Hubs, Middleware, Filters
│   ├── HealthSync.Core/              # Entities, Enums, DTOs, Interfaces, Services
│   └── HealthSync.Infrastructure/    # DbContext, Repositories, Seeder, Migrations
├── HealthSync.sln
└── Dockerfile

frontend/
├── src/
│   ├── components/                   # Common, Layout, Auth, Appointment, Domain
│   ├── pages/                        # Dashboard, Login, Patients, Appointments, etc.
│   ├── services/                     # Axios API client + service modules
│   ├── hooks/                        # useAuth, useSignalR, usePagination, useDebounce
│   ├── contexts/                     # AuthContext (React Context)
│   ├── types/                        # TypeScript interfaces and enums
│   └── realtime/                     # SignalR hub connection + handlers
├── package.json
├── vite.config.ts
└── Dockerfile
```

## Key Design Decisions

- **Clean Architecture**: Business logic in Core has zero external dependencies. Infrastructure implements Core's interfaces. API depends on both but only wires things up.
- **Enum-as-string storage**: Enums stored as varchar in PostgreSQL (via `HasConversion<string>()`) for readability over native PG enums.
- **Guid primary keys**: Consistent UUIDs across all tables, generated client-side or via `gen_random_uuid()`.
- **15-minute appointment slots**: Enforced at the service layer, with overlapping-slot validation against existing appointments and time-off records.
- **JWT with refresh token rotation**: Short-lived access tokens (15 min) + long-lived refresh tokens (7 days) stored in the user table. Each refresh invalidates the previous token (rotation). Password hashing uses `PasswordHasher<T>` directly (no Identity framework).
- **Nullable UserId on Patient**: Walk-in patients can be registered without requiring a portal login account.
- **Port 5433 for Docker PostgreSQL**: Avoids conflict with local PostgreSQL on 5432 — both can run simultaneously.

## License

MIT
