import {
  Boxes,
  DatabaseZap,
  LayoutDashboard,
  Orbit,
  PackageCheck,
  ShieldCheck,
} from "lucide-react";
import type {
  DashboardFoundationCard,
  DeliveryLane,
} from "@/features/dashboard/types/dashboard.types";

export const dashboardFoundationCards: DashboardFoundationCard[] = [
  {
    title: "App Router structure",
    description: "Shared shell, route group layout, and module placeholders are wired.",
    metric: "7 routes",
    icon: LayoutDashboard,
    accent: "indigo",
  },
  {
    title: "Design system foundation",
    description: "Dark premium tokens, card surfaces, and reusable primitives are ready.",
    metric: "5 primitives",
    icon: Boxes,
    accent: "blue",
  },
  {
    title: "Data integration layer",
    description: "React Query provider and API abstraction are prepared for backend hookup.",
    metric: "Query-ready",
    icon: DatabaseZap,
    accent: "green",
  },
  {
    title: "State management",
    description: "Zustand powers shell state and leaves room for future domain stores.",
    metric: "Shell store",
    icon: Orbit,
    accent: "orange",
  },
];

export const deliveryLanes: DeliveryLane[] = [
  {
    phase: "Phase 2",
    title: "UI System",
    summary: "Extend the primitive set with tables, selects, modals, and form affordances.",
  },
  {
    phase: "Phase 3",
    title: "Dashboard",
    summary: "Ship KPI cards, recent bookings, activity feed, quick actions, and occupancy.",
  },
  {
    phase: "Phase 4-8",
    title: "Operations Modules",
    summary: "Implement rooms, bookings, check-in, housekeeping, payments, and invoices.",
  },
  {
    phase: "Phase 9+",
    title: "Security & polish",
    summary: "Add auth, role protection, notifications, and tighter backend integration.",
  },
];

export const foundationHighlights = [
  {
    title: "Scalable feature layout",
    description:
      "Every module has its own `components`, `data`, and `types` folder to keep growth local.",
    icon: PackageCheck,
  },
  {
    title: "Backend integration ready",
    description:
      "The API client is isolated under `src/services`, keeping UI code decoupled from transport.",
    icon: ShieldCheck,
  },
];
