import { ShieldCheck } from "lucide-react";

export const rolesPhasePreview = {
  icon: ShieldCheck,
  title: "User Roles",
  phase: "Phase 9",
  summary:
    "Role cards, permissions visibility, and role-aware UI states are reserved for the authentication and access-control phase.",
  accent: "red" as const,
  deliverables: [
    "Role cards with icon and description",
    "Permission lists by role",
    "Selected role state handling",
    "Access-aware UI hooks",
  ],
  foundationItems: [
    "State management with Zustand",
    "Dedicated feature folder and route",
    "Reusable card and badge primitives",
    "Scaffold ready for protected layouts later",
  ],
};
