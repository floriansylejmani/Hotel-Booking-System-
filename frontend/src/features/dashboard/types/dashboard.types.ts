import type { LucideIcon } from "lucide-react";

export type DashboardFoundationCard = {
  title: string;
  description: string;
  metric: string;
  icon: LucideIcon;
  accent: "blue" | "indigo" | "green" | "orange";
};

export type DeliveryLane = {
  phase: string;
  title: string;
  summary: string;
};
