import { ScanSearch } from "lucide-react";

export const checkinPhasePreview = {
  icon: ScanSearch,
  title: "Check-in / Check-out",
  phase: "Phase 6",
  summary:
    "Booking verification, stay transitions, and daily summary cards will be implemented when the operational workflows are built.",
  accent: "green" as const,
  deliverables: [
    "Search and empty-state handling",
    "Check-in and check-out stat cards",
    "Booking result workflow",
    "Status transition controls",
  ],
  foundationItems: [
    "Validated search form pattern",
    "Header + sidebar application frame",
    "Prepared service abstraction",
    "Feature route isolation",
  ],
};
