import type { Permission } from "@/types/hotel";
import type { LucideIcon } from "lucide-react";
import {
  BedDouble,
  CalendarDays,
  ClipboardCheck,
  CreditCard,
  LayoutDashboard,
  ShieldCheck,
  Sparkles,
} from "lucide-react";

export type NavigationItem = {
  href: string;
  label: string;
  caption: string;
  phase: string;
  icon: LucideIcon;
  permission: Permission;
};

export const mainNavigation: NavigationItem[] = [
  {
    href: "/dashboard",
    label: "Dashboard",
    caption: "Executive overview",
    phase: "P3",
    icon: LayoutDashboard,
    permission: "dashboard.view",
  },
  {
    href: "/rooms",
    label: "Rooms",
    caption: "Inventory workspace",
    phase: "P4",
    icon: BedDouble,
    permission: "rooms.manage",
  },
  {
    href: "/bookings",
    label: "Bookings",
    caption: "Reservation workflows",
    phase: "P5",
    icon: CalendarDays,
    permission: "bookings.manage",
  },
  {
    href: "/checkin",
    label: "Check-in / Out",
    caption: "Stay transitions",
    phase: "P6",
    icon: ClipboardCheck,
    permission: "checkin.manage",
  },
  {
    href: "/housekeeping",
    label: "Housekeeping",
    caption: "Room readiness",
    phase: "P7",
    icon: Sparkles,
    permission: "housekeeping.manage",
  },
  {
    href: "/payments",
    label: "Payments",
    caption: "Invoices and billing",
    phase: "P8",
    icon: CreditCard,
    permission: "payments.manage",
  },
  {
    href: "/roles",
    label: "User Roles",
    caption: "Access model",
    phase: "P9",
    icon: ShieldCheck,
    permission: "roles.manage",
  },
];

export const pageTitles: Record<string, string> = {
  "/dashboard": "Dashboard",
  "/rooms": "Room Management",
  "/bookings": "Bookings",
  "/checkin": "Check-in / Check-out",
  "/housekeeping": "Housekeeping",
  "/payments": "Payments & Invoices",
  "/roles": "User Roles",
  "/login": "Sign In",
  "/register": "Create Account",
};
