# DATABASE DESIGN

## Engine

PostgreSQL

---

## PHASE 1 — CORE TABLES

### users

- id
- name
- email
- password_hash
- role_id

### roles

- id
- name

---

## PHASE 2 — ROOMS

### rooms

- id
- room_number
- type
- status
- price

---

## PHASE 3 — BOOKINGS

### bookings

- id
- user_id
- room_id
- check_in
- check_out
- status

---

## PHASE 4 — HOUSEKEEPING

### housekeeping_logs

- id
- room_id
- status
- assigned_to
- updated_at

---

## PHASE 5 — PAYMENTS

### invoices

- id
- booking_id
- total
- tax

### payments

- id
- invoice_id
- method
- amount

---

## PHASE 6 — NOTIFICATIONS

### notifications

- id
- user_id
- message
- type
- is_read

---

## RELATIONSHIPS

- users → bookings
- rooms → bookings
- bookings → invoices
- invoices → payments
- rooms → housekeeping_logs
