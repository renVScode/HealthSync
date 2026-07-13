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
- **Appointment scheduling** — 15-minute slots, availability rules, time-off blocking, status lifecycle. Month-only calendar with styled event cards; click an event for a detail modal showing patient, time, reason, and status.
- **Patient management** — Full CRUD with search, medical history timeline (collapsible), allergies, emergency contacts
- **Electronic Medical Records** — Diagnoses, prescriptions, treatment plans, confidentiality flags
- **Pharmacy inventory** — Batch tracking with expiry dates, stock movements (stock-in, dispensing, adjustments), reorder alerts. Queue view with search.
- **Billing & payments** — Invoicing with line items, multi-payment support (Cash/Card/Online/Insurance), partial payments, conditional per-method fields (Card — cardholder/last4/approval code; Online — gateway/reference; Insurance — provider/policy/auth/coverage)
- **Reporting** — Aggregated analytics across appointments, revenue, inventory with date range filtering
- **Archive & restore** — Soft-delete users, patients, and medical records; view and restore them from a dedicated Archives page with search. Archived users are automatically set inactive.
- **Custom confirmation dialogs** — A reusable `ConfirmDialog` modal component replaces native `window.confirm()` across all destructive actions.
- **Doctor service offerings** — Per-doctor service catalog (ECG, wound dressing, etc.) with pricing, active/inactive toggle. Managed via a dedicated Services tab on DoctorDetail.
- **Lab tests module** — Catalog of lab tests with prices (Admin-managed), test ordering by Doctors, status lifecycle (Ordered→Collected→Processing→Completed), result entry with reference ranges
- **Real-time updates** — Live appointment changes and push notifications via SignalR
- **Audit logging** — All create/update/delete operations tracked with JSON snapshots
- **Search across all tables** — Debounced search on Patients, Users, Appointments, Pharmacy, Archives, Lab Tests, and more
- **Icon-based action buttons** — Tables use icon buttons (edit, delete, toggle, archive, restore) instead of text labels for compact, clean UI
- **Responsive sidebar** — Slides off-screen on mobile with hamburger toggle + overlay
- **Dashboard date filter** — From/To date inputs with Apply and Reset to filter KPIs and charts
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

### Domain Model (17 entities)

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
                ──1:M── DoctorServiceOfferings
                ──1:M── LabOrders

LabTests ──1:M── LabOrders ──M:1── Patients
                         ──M:1── Doctors

Medicines ──1:M── InventoryBatches ──1:M── InventoryTransactions

Users ──1:0..1── Patients
Users ──1:0..1── Doctors
```
*All entities use `Guid` primary keys with `gen_random_uuid()`.*

## Live Deployment (Render)

The app is deployed and accessible at:

| Service | URL |
|---|---|
| Frontend | https://healthsync-frontend-oc2s.onrender.com |
| API | https://healthsync-y27c.onrender.com |
| Swagger | https://healthsync-y27c.onrender.com/swagger |

The frontend uses **HashRouter** (`/#/path` URLs) for SPA routing — no server-side rewrites required.

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

## API Overview (11 Controllers)

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
| `GET/POST/PUT/DELETE /api/labtests` | Lab test catalog (Admin) |
| `GET/POST/PUT/DELETE /api/labtests/orders` | Lab order lifecycle + result entry |
| `GET/POST/PUT/DELETE /api/doctors/{id}/services` | Per-doctor service offerings |
| `PATCH /api/patients/{id}/archive\|restore` | Archive or restore a patient |
| `PATCH /api/users/{id}/archive\|restore` | Archive or restore a user |
| `PATCH /api/medical-records/{id}/archive\|restore` | Archive or restore a medical record |

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
│   ├── pages/                        # Dashboard, Login, Patients, Appointments, Archives, etc.
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
- **Soft-delete via IsArchived flag**: Archived records hide from main views (filtered where `IsArchived = false`) and reappear only in the Archives page. Archived users also get `IsActive = false`.

## License

MIT
