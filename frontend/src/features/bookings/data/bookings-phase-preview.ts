import { CalendarDays } from "lucide-react";

export const bookingsPhasePreview = {
  icon: CalendarDays,
  title: "Bookings",
  phase: "Phase 5",
  summary:
    "Search, table views, status badges, and booking drill-down flows will be added in the bookings delivery phase.",
  accent: "indigo" as const,
  deliverables: [
    "Reservation table and row actions",
    "Guest or booking ID search",
    "Booking and payment status filters",
    "View details workflow",
  ],
  foundationItems: [
    "Feature-based route scaffold",
    "Global header search form",
    "Shared layout and responsive spacing",
    "Backend-ready services folder",
  ],
};
