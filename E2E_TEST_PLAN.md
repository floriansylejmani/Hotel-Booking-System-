# Hotel Booking System E2E Test Plan

Playwright is not currently installed in this project. This plan documents the minimal end-to-end regression path to automate if browser E2E is added later.

## Prerequisites

- Start PostgreSQL and API with `docker compose up -d`.
- Start frontend with `cd frontend && npm run dev`.
- Confirm API health endpoint returns healthy.

## Scenarios

1. Guest registration
   - Open `/register`.
   - Fill full name, unique email, password, and matching confirmation.
   - Submit.
   - Expected: user is signed in and redirected to dashboard.

2. Guest login
   - Log out.
   - Open `/login`.
   - Sign in with the newly registered guest credentials.
   - Expected: token/session is stored and protected UI loads.

3. Rooms browsing
   - Navigate to rooms if the role is allowed, or use a staff/admin account.
   - Expected: room list renders prices, status, floor, bed count, and amenities.

4. Booking creation
   - Navigate to bookings.
   - Create a booking with a valid room and future date range.
   - Expected: booking appears in the booking list with confirmed status and correct total.

5. Admin dashboard
   - Log out and sign in as `admin@hotel.com` / `Admin123!`.
   - Open `/dashboard`.
   - Expected: summary metrics, occupancy data, recent bookings, and activity feed render with non-negative values.

6. Check-in/check-out
   - Open `/checkin`.
   - Check in the confirmed booking created above.
   - Check out the same booking.
   - Expected: booking becomes checked out and room moves to cleaning/housekeeping state.

7. Payment
   - Open `/payments`.
   - Select the booking.
   - Process payment with valid card details.
   - Expected: success toast appears and invoice/payment data refreshes.

8. Invoice
   - On the payments screen, click invoice download.
   - Expected: PDF download is triggered for the selected invoice.

9. Logout
   - Use the logout action.
   - Expected: auth state clears and protected pages redirect to login.
