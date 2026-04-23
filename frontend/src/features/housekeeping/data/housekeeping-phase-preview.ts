import { Sparkles } from "lucide-react";

export const housekeepingPhasePreview = {
  icon: Sparkles,
  title: "Housekeeping",
  phase: "Phase 7",
  summary:
    "Cleaner assignment, room status actions, and maintenance notes will be implemented once the operational screens start landing.",
  accent: "orange" as const,
  deliverables: [
    "Summary cards by housekeeping state",
    "Cleaner assignment and last-cleaned info",
    "Maintenance note surfaces",
    "Room status action controls",
  ],
  foundationItems: [
    "Premium dark card system",
    "Status badge primitive",
    "Service abstraction for future APIs",
    "Route scaffolding and shared shell",
  ],
};
