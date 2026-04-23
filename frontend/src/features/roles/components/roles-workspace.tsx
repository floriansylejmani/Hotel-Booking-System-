"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Building2, Power, ShieldCheck, Sparkles, User } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { SectionHeading } from "@/components/ui/section-heading";
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from "@/components/ui/table";
import { activateStaff, deactivateStaff, getStaff } from "@/features/roles/api";
import { useAuth } from "@/hooks/use-auth";
import { formatRoleLabel } from "@/lib/format";
import { rolePermissions } from "@/lib/permissions";
import { getApiErrorMessage } from "@/services/api-client";
import type { RoleDefinition, UserRole } from "@/types/hotel";

const roleIcons: Record<UserRole, typeof Building2> = {
  admin: ShieldCheck,
  manager: Building2,
  receptionist: User,
  housekeeper: Sparkles,
  guest: User,
};

const roleBadgeVariant: Record<UserRole, "indigo" | "blue" | "green" | "orange"> = {
  admin: "indigo",
  manager: "blue",
  receptionist: "green",
  housekeeper: "orange",
  guest: "indigo",
};

export function RolesWorkspace() {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const isManager = user?.role === "admin" || user?.role === "manager";

  const staffQuery = useQuery({
    queryKey: ["staff"],
    queryFn: () => getStaff({ pageSize: 50 }),
    enabled: isManager,
  });

  const deactivateMutation = useMutation({
    mutationFn: deactivateStaff,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["staff"] }),
  });

  const activateMutation = useMutation({
    mutationFn: activateStaff,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["staff"] }),
  });

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Staff & Roles"
        title="User Roles"
        description="Role definitions, permissions, and active staff members across hotel operations."
        actions={
          <Badge variant="indigo" size="md">
            Active role: {formatRoleLabel(user?.role ?? "guest")}
          </Badge>
        }
      />

      <div className="grid gap-5 xl:grid-cols-4">
        {roleDefinitions.map((role) => (
          <RoleCard key={role.role} isActive={role.role === user?.role} role={role} />
        ))}
      </div>

      {isManager && (
        <Card>
          <CardHeader>
            <CardTitle>Staff Members</CardTitle>
            <CardDescription>
              All hotel staff accounts. Managers and admins can activate or deactivate accounts.
            </CardDescription>
          </CardHeader>
          <CardContent>
            {staffQuery.isLoading && (
              <p className="text-sm text-muted-foreground">Loading staff...</p>
            )}
            {staffQuery.isError && (
              <p className="text-sm text-accent-red">
                {getApiErrorMessage(staffQuery.error, "Failed to load staff.")}
              </p>
            )}
            {staffQuery.data && (
              <Table>
                <TableHead>
                  <tr>
                    <TableHeaderCell>Name</TableHeaderCell>
                    <TableHeaderCell>Email</TableHeaderCell>
                    <TableHeaderCell>Role</TableHeaderCell>
                    <TableHeaderCell>Status</TableHeaderCell>
                    <TableHeaderCell className="text-right">Actions</TableHeaderCell>
                  </tr>
                </TableHead>
                <TableBody>
                  {staffQuery.data.items.length ? (
                    staffQuery.data.items.map((member) => (
                      <TableRow key={member.id}>
                        <TableCell className="font-semibold text-white">
                          {member.fullName}
                        </TableCell>
                        <TableCell className="text-muted-foreground">{member.email}</TableCell>
                        <TableCell>
                          <Badge variant={roleBadgeVariant[member.role]} size="sm">
                            {formatRoleLabel(member.role)}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <Badge variant={member.isActive ? "green" : "indigo"} size="sm">
                            {member.isActive ? "Active" : "Inactive"}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-right">
                          {member.role !== "admin" && (
                            <Button
                              size="sm"
                              variant="secondary"
                              onClick={() =>
                                member.isActive
                                  ? deactivateMutation.mutate(member.id)
                                  : activateMutation.mutate(member.id)
                              }
                              disabled={deactivateMutation.isPending || activateMutation.isPending}
                            >
                              <Power className="mr-1.5 size-3.5" />
                              {member.isActive ? "Deactivate" : "Activate"}
                            </Button>
                          )}
                        </TableCell>
                      </TableRow>
                    ))
                  ) : (
                    <TableRow>
                      <TableCell colSpan={5} className="text-center text-muted-foreground">
                        No staff members found.
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}

function RoleCard({ isActive, role }: { isActive: boolean; role: RoleDefinition }) {
  const Icon = roleIcons[role.role];
  return (
    <Card
      className={
        isActive
          ? "border-accent-indigo/35 bg-[linear-gradient(180deg,rgba(45,27,78,0.55),rgba(7,11,20,0.96))]"
          : undefined
      }
    >
      <CardHeader className="space-y-4">
        <div className="flex size-14 items-center justify-center rounded-[1.4rem] bg-[linear-gradient(135deg,#5b7cff,#7c63ff)] text-white">
          <Icon className="size-6" />
        </div>
        <div>
          <CardTitle>{role.title}</CardTitle>
          <CardDescription>{role.description}</CardDescription>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-3">
          {role.permissions.map((permission) => (
            <div key={permission} className="flex items-center gap-3 text-sm text-slate-200">
              <span className="size-1.5 rounded-full bg-accent-cyan" />
              {permission}
            </div>
          ))}
        </div>
        <div className="mt-6 rounded-[1rem] border border-surface-border bg-white/[0.03] px-4 py-3 text-sm text-muted-foreground">
          {isActive
            ? "Current role comes from the backend JWT session."
            : "Available when a matching backend user logs in."}
        </div>
      </CardContent>
    </Card>
  );
}

const roleDefinitions: RoleDefinition[] = [
  {
    role: "admin",
    title: "Administrator",
    description: "Full access across hotel operations, configuration, and oversight.",
    permissions: rolePermissions.admin,
    tone: "indigo",
  },
  {
    role: "manager",
    title: "Manager",
    description: "Operational oversight across rooms, bookings, billing, and housekeeping.",
    permissions: rolePermissions.manager,
    tone: "blue",
  },
  {
    role: "receptionist",
    title: "Receptionist",
    description: "Front-desk workflows for bookings, arrivals, departures, and invoicing.",
    permissions: rolePermissions.receptionist,
    tone: "green",
  },
  {
    role: "housekeeper",
    title: "Housekeeper",
    description: "Focused access to housekeeping tasks and room readiness operations.",
    permissions: rolePermissions.housekeeper,
    tone: "orange",
  },
  {
    role: "guest",
    title: "Guest",
    description: "Limited self-service access based on authenticated guest permissions.",
    permissions: rolePermissions.guest,
    tone: "indigo",
  },
];
