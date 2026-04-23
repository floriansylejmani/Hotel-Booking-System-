import { BedDouble } from "lucide-react";

export const roomsPhasePreview = {
  icon: BedDouble,
  title: "Room Management",
  phase: "Phase 4",
  summary:
    "Room inventory, filters, CRUD flows, and room detail cards will land here once the shared system is fully hardened.",
  accent: "blue" as const,
  deliverables: [
    "Type and status filter controls",
    "Room card grid with occupancy state",
    "Add, edit, and remove room workflows",
    "Reusable room metadata blocks",
  ],
  foundationItems: [
    "Shared shell and navigation",
    "Reusable card, badge, button, and input primitives",
    "Zustand shell state",
    "React Query provider and service layer",
  ],
};
