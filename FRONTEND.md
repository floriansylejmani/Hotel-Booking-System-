# FRONTEND DEVELOPMENT PLAN

## Tech Stack

- Next.js (App Router)
- TypeScript
- Tailwind CSS
- React Query
- Zustand

---

## PHASE 1 — PROJECT SETUP

### Tasks

- Create Next.js app
- Setup Tailwind CSS
- Setup folder structure
- Setup layout (sidebar + header)

### Structure

src/

- app/
- components/
- features/
- services/
- store/
- hooks/

---

## PHASE 2 — UI SYSTEM

### Components

- Button
- Card
- Badge
- Table
- Modal
- Input
- Select

### Design Rules

- Dark theme
- Rounded cards
- Soft shadows
- Gradient highlights

---

## PHASE 3 — DASHBOARD

### Pages

- Dashboard

### Features

- KPI Cards (Rooms, Bookings, Revenue)
- Recent Bookings Table
- Activity Feed
- Quick Actions

---

## PHASE 4 — ROOM MANAGEMENT

### Pages

- /rooms

### Features

- Room cards grid
- Filters (type, status)
- Add Room modal
- Edit/Delete room

---

## PHASE 5 — BOOKINGS

### Pages

- /bookings

### Features

- Table view
- Search (ID, guest)
- Status filters
- Booking details

---

## PHASE 6 — CHECK-IN / CHECK-OUT

### Pages

- /checkin

### Features

- Search booking
- Verify booking
- Assign room key
- Update status

---

## PHASE 7 — HOUSEKEEPING

### Pages

- /housekeeping

### Features

- Room cleaning status
- Assign staff
- Update status (Clean / Dirty / In Progress)

---

## PHASE 8 — PAYMENTS

### Pages

- /payments

### Features

- Invoice UI
- Payment methods
- Process payment
- Download PDF

---

## PHASE 9 — AUTH & ROLES

### Features

- Login / Register
- Role-based UI
- Protected routes

---

## PHASE 10 — NOTIFICATIONS

### Features

- Notification dropdown
- Toast alerts
- Real-time updates (optional)
