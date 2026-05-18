# Hotel Booking System

![CI](https://github.com/floriansylejmani/Hotel-Booking-System-/actions/workflows/ci.yml/badge.svg)
![Next.js](https://img.shields.io/badge/Next.js-16-black?logo=next.js)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)
![ASP.NET](https://img.shields.io/badge/ASP.NET_Core-9-512BD4?logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Database-336791?logo=postgresql)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript)
![TailwindCSS](https://img.shields.io/badge/TailwindCSS-4-38BDF8?logo=tailwindcss)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)
![Tests](https://img.shields.io/badge/Tests-126_Passing-success)
![License](https://img.shields.io/badge/License-Portfolio-blue)

Production-candidate full-stack hotel management platform built with ASP.NET Core, PostgreSQL, Next.js, React, TypeScript, Tailwind CSS, Docker Compose, and GitHub Actions.

The project models the core workflows of a real hotel operation: authentication, role-based access, room inventory, bookings, check-in/check-out, payments, invoices, housekeeping, notifications, and operational dashboard metrics.

## Status

- Backend build: passing
- Frontend build: passing
- CI/CD: GitHub Actions enabled
- Docker Compose: PostgreSQL and API ready
- Automated tests: 126 passing
- Backend tests: 85 passing
- Frontend tests: 41 passing
- Coverage reporting: enabled for backend and frontend
- Security and business-rule tests: included

## Tech Stack

### Backend

- ASP.NET Core 9 Web API
- Clean Architecture
- Entity Framework Core
- PostgreSQL
- JWT authentication
- Role-based authorization
- xUnit test suite
- WebApplicationFactory API tests

### Frontend

- Next.js 16 App Router
- React 19
- TypeScript
- Tailwind CSS
- TanStack Query
- Zustand
- Vitest
- React Testing Library

### DevOps

- Docker Compose
- PostgreSQL service container
- GitHub Actions CI
- Backend and frontend coverage artifacts

## Features

- User registration and login with JWT-based sessions
- Role-based access for guests, staff, and administrators
- Room management with pricing, capacity, type, and status tracking
- Availability search with booking conflict protection
- Booking lifecycle management and validation
- Check-in and check-out workflows
- Payment processing simulation and invoice generation
- Housekeeping task creation, assignment, and status updates
- Notifications for user-facing operational events
- Dashboard metrics for revenue, occupancy, rooms, and bookings
- API validation, authorization, and security edge-case coverage

## Architecture

The backend follows Clean Architecture boundaries:

```text
backend/
  src/
    HotelBooking.API
    HotelBooking.Application
    HotelBooking.Domain
    HotelBooking.Infrastructure
    HotelBooking.Persistence
  tests/
    HotelBooking.UnitTests
    HotelBooking.ApiTests
    HotelBooking.IntegrationTests
```

The frontend uses a feature-oriented structure:

```text
frontend/
  src/
    app/
    components/
    features/
    hooks/
    lib/
    services/
    store/
    test/
    types/
```

## Getting Started

### Prerequisites

- .NET SDK 9
- Node.js 22 or later
- npm
- Docker Desktop
- PostgreSQL, or Docker Compose for the bundled database

### Environment

Copy the example environment file and set local values:

```powershell
Copy-Item .env.example .env
```

The committed `.env.example` contains only safe placeholder values. Do not commit real secrets, production credentials, or local `.env` files.

Required values:

```text
NEXT_PUBLIC_API_BASE_URL=http://localhost:5259/api
JWT_SECRET=replace-with-a-strong-local-development-secret
POSTGRES_DB=hotel_booking
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
```

`JWT_SECRET` must be at least 32 characters for local and containerized API startup.

## Run With Docker

Start PostgreSQL and the API:

```powershell
docker compose up --build
```

API URL:

```text
http://localhost:8080
```

Health check:

```text
http://localhost:8080/health
```

## Run Locally

### Backend

```powershell
cd backend
dotnet restore
dotnet build
dotnet run --project src/HotelBooking.API
```

Default local API URL:

```text
http://localhost:5259
```

### Frontend

```powershell
cd frontend
npm ci
npm run dev
```

Default frontend URL:

```text
http://localhost:3000
```

## Seeded Users

Development seed data includes representative users for local testing:

| Role | Email | Password |
| --- | --- | --- |
| Admin | admin@hotel.com | Admin123! |
| Guest | james.carter@example.com | Guest123! |
| Staff | maria.santos@hotel.com | Staff123! |

These credentials are for development seed data only.

## Testing

### Backend

```powershell
cd backend
dotnet test
```

Backend coverage:

```powershell
cd backend
dotnet test --collect:"XPlat Code Coverage"
```

Backend test coverage includes:

- Application service unit tests
- Business-rule edge cases
- API integration tests with WebApplicationFactory
- Authorization and security tests
- EF Core persistence and database behavior tests

### Frontend

```powershell
cd frontend
npm run lint
npm run test
npm run build
```

Frontend coverage:

```powershell
cd frontend
npm run test:coverage
```

Frontend tests include:

- Authentication UI flows
- Protected route behavior
- Role-based navigation behavior
- Room listing states
- Booking form validation and submission
- Admin dashboard behavior
- Payment and invoice states
- Notification interactions
- API client and auth store behavior

## CI/CD

GitHub Actions runs on pushes and pull requests:

- Backend restore, build, test, and coverage collection
- Frontend install, lint, test coverage, build, and coverage upload
- PostgreSQL service container for backend integration coverage

Workflow file:

```text
.github/workflows/ci.yml
```

## Security Notes

- JWT secrets are not committed.
- `.env` and `.env.local` files are ignored.
- API configuration requires a strong JWT key.
- Role-based authorization is covered by automated tests.
- Users are blocked from accessing other users' bookings, payments, invoices, and notifications.
- Admin and staff endpoint access is covered by API tests.
- SQL injection-like and XSS-like inputs are covered by validation/security tests.

## Repository Hygiene

Generated folders and local-only files are ignored:

- `node_modules/`
- `.next/`
- `coverage/`
- `TestResults/`
- `bin/`
- `obj/`
- `logs/`
- `*.log`
- `.lscache/`
- `.env`
- `.env.local`

## License

MIT
