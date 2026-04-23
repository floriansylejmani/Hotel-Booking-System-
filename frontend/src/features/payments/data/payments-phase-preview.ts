import { CreditCard } from "lucide-react";

export const paymentsPhasePreview = {
  icon: CreditCard,
  title: "Payments & Invoices",
  phase: "Phase 8",
  summary:
    "Invoice layouts, payment forms, and billing actions are staged for the payments delivery phase after the operational modules land.",
  accent: "green" as const,
  deliverables: [
    "Invoice summary layout and line items",
    "Payment method selector and form inputs",
    "Process payment action",
    "Print and PDF controls",
  ],
  foundationItems: [
    "Form tooling with React Hook Form + Zod",
    "Reusable button and input primitives",
    "React Query provider for mutation flows",
    "Backend-ready API client",
  ],
};
