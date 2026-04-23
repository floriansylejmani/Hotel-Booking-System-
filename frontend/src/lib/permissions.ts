import type { Permission, UserRole } from "@/types/hotel";

export const rolePermissions: Record<UserRole, Permission[]> = {
  admin: [
    "dashboard.view",
    "rooms.manage",
    "bookings.manage",
    "checkin.manage",
    "housekeeping.manage",
    "payments.manage",
    "roles.manage",
  ],
  manager: [
    "dashboard.view",
    "rooms.manage",
    "bookings.manage",
    "checkin.manage",
    "housekeeping.manage",
    "payments.manage",
    "roles.manage",
  ],
  receptionist: [
    "dashboard.view",
    "rooms.manage",
    "bookings.manage",
    "checkin.manage",
    "payments.manage",
  ],
  housekeeper: ["dashboard.view", "housekeeping.manage"],
  guest: ["dashboard.view"],
};

export function hasPermission(role: UserRole, permission: Permission) {
  return rolePermissions[role].includes(permission);
}

const routePermissions: Array<{ prefix: string; permission: Permission }> = [
  { prefix: "/dashboard", permission: "dashboard.view" },
  { prefix: "/rooms", permission: "rooms.manage" },
  { prefix: "/bookings", permission: "bookings.manage" },
  { prefix: "/checkin", permission: "checkin.manage" },
  { prefix: "/housekeeping", permission: "housekeeping.manage" },
  { prefix: "/payments", permission: "payments.manage" },
  { prefix: "/roles", permission: "roles.manage" },
];

export function getRequiredPermission(pathname: string) {
  return routePermissions.find((route) => pathname.startsWith(route.prefix))
    ?.permission;
}
