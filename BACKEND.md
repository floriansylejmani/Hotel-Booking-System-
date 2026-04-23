# BACKEND DEVELOPMENT PLAN

## Tech Stack

- ASP.NET Core Web API
- Clean Architecture
- JWT Authentication
- Entity Framework Core

---

## PHASE 1 — SETUP

### Tasks

- Create solution
- Setup layers:
  - API
  - Application
  - Domain
  - Infrastructure
  - Persistence

---

## PHASE 2 — AUTHENTICATION

### Features

- Register
- Login
- JWT Token
- Role system

---

## PHASE 3 — ROOM MODULE

### Endpoints

- GET /rooms
- POST /rooms
- PUT /rooms/{id}
- DELETE /rooms/{id}

### Logic

- Room types
- Room status

---

## PHASE 4 — BOOKING MODULE

### Endpoints

- POST /bookings
- GET /bookings
- PUT /bookings/{id}/cancel

### Logic

- Check availability
- Prevent double booking

---

## PHASE 5 — CHECK-IN / CHECK-OUT

### Endpoints

- PUT /bookings/{id}/check-in
- PUT /bookings/{id}/check-out

### Logic

- Assign room
- Update status

---

## PHASE 6 — HOUSEKEEPING

### Endpoints

- GET /housekeeping
- PUT /housekeeping/{id}

---

## PHASE 7 — PAYMENTS & INVOICES

### Endpoints

- POST /payments
- GET /invoices/{bookingId}

### Logic

- Calculate total
- Tax
- Room charges

---

## PHASE 8 — NOTIFICATIONS

### Events

- Booking created
- Booking cancelled
- Payment completed

---

## PHASE 9 — ERROR HANDLING

- Global exception middleware
- Validation errors
- Logging
