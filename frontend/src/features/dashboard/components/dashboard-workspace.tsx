"use client";

import { useQuery } from "@tanstack/react-query";
import {
  BedDouble,
  CalendarCheck2,
  CircleDollarSign,
  DoorOpen,
  HousePlus,
  Sparkles,
} from "lucide-react";
import { useRouter } from "next/navigation";
import { MetricCard } from "@/components/shared/metric-card";
import { PageLoader } from "@/components/shared/page-loader";
import { StatusBadge } from "@/components/shared/status-badge";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { SectionHeading } from "@/components/ui/section-heading";
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from "@/components/ui/table";
import { getDashboardData } from "@/features/dashboard/api";
import { useAuth } from "@/hooks/use-auth";
import { formatCompactDate, formatCurrency } from "@/lib/format";
import { getApiErrorMessage } from "@/services/api-client";

const kpiIcons = [BedDouble, CalendarCheck2, DoorOpen, CircleDollarSign] as const;
const quickActionIcons = [CalendarCheck2, DoorOpen, HousePlus, Sparkles] as const;

const quickActionRoutes: Record<string, string> = {
  "new-booking": "/bookings",
  "checkin-desk": "/checkin",
  "housekeeping-board": "/housekeeping",
  billing: "/payments",
};

export function DashboardWorkspace() {
  const router = useRouter();
  const { user } = useAuth();
  const dashboardQuery = useQuery({
    queryKey: ["dashboard"],
    queryFn: getDashboardData,
  });

  if (dashboardQuery.isLoading) {
    return <PageLoader label="Loading dashboard metrics..." />;
  }

  if (dashboardQuery.isError) {
    return (
      <div className="rounded-[1.5rem] border border-accent-red/20 bg-accent-red/10 px-6 py-5 text-sm text-accent-red">
        {getApiErrorMessage(dashboardQuery.error, "Unable to load dashboard data.")}
      </div>
    );
  }

  const dashboardData = dashboardQuery.data;
  if (!dashboardData) {
    return <PageLoader label="Loading dashboard metrics..." />;
  }

  const { activities, kpis, occupancy, quickActions, recentBookings } = dashboardData;

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Phase 10"
        title={`Welcome back, ${user?.name?.split(" ")[0] ?? "Operator"}`}
        description="Today's operations snapshot now comes from the live backend and reflects occupancy, revenue, reservations, and recent notification activity."
        actions={
          <Badge variant="indigo" size="md">
            Hotel ops live
          </Badge>
        }
      />

      <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-4">
        {kpis.map((item, index) => {
          const Icon = kpiIcons[index];
          return (
            <MetricCard
              key={item.label}
              icon={Icon}
              label={item.label}
              value={item.value}
              sublabel={item.sublabel}
              delta={item.delta}
              tone={item.tone}
            />
          );
        })}
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.6fr)_minmax(0,0.9fr)]">
        <Card>
          <CardHeader className="flex flex-row items-start justify-between gap-4">
            <div>
              <CardTitle>Recent Bookings</CardTitle>
              <CardDescription>Latest reservations across the hotel.</CardDescription>
            </div>
            <Button variant="secondary" size="sm" onClick={() => router.push("/bookings")}>
              View all
            </Button>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHead>
                <tr>
                  <TableHeaderCell>Booking ID</TableHeaderCell>
                  <TableHeaderCell>Guest</TableHeaderCell>
                  <TableHeaderCell>Room</TableHeaderCell>
                  <TableHeaderCell>Check-in</TableHeaderCell>
                  <TableHeaderCell>Amount</TableHeaderCell>
                  <TableHeaderCell>Status</TableHeaderCell>
                </tr>
              </TableHead>
              <TableBody>
                {recentBookings.length ? (
                  recentBookings.map((booking) => (
                    <TableRow key={booking.recordId}>
                      <TableCell className="font-semibold text-[#9ea7ff]">
                        {booking.id}
                      </TableCell>
                      <TableCell className="font-semibold text-white">
                        {booking.guestName}
                      </TableCell>
                      <TableCell>{`Room ${booking.roomNumber}`}</TableCell>
                      <TableCell>{formatCompactDate(booking.checkIn)}</TableCell>
                      <TableCell className="font-semibold text-accent-green">
                        {formatCurrency(booking.amount)}
                      </TableCell>
                      <TableCell>
                        <StatusBadge status={booking.status} />
                      </TableCell>
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={6} className="text-center text-muted-foreground">
                      No booking activity yet.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Activity Feed</CardTitle>
              <CardDescription>Most recent hotel operations activity.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {activities.length ? (
                activities.map((activity) => (
                  <div
                    key={activity.id}
                    className="flex items-start gap-3 rounded-[1.25rem] border border-surface-border bg-white/[0.02] px-4 py-3"
                  >
                    <div className="mt-1 size-2 rounded-full bg-accent-cyan" />
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-semibold text-white">{activity.title}</p>
                      <p className="mt-1 text-sm text-muted-foreground">
                        {activity.description}
                      </p>
                    </div>
                    <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
                      {activity.timeAgo}
                    </p>
                  </div>
                ))
              ) : (
                <p className="text-sm text-muted-foreground">
                  No notification activity is available yet.
                </p>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Quick Actions</CardTitle>
              <CardDescription>Shortcuts for the most common desk workflows.</CardDescription>
            </CardHeader>
            <CardContent className="grid gap-3 sm:grid-cols-2">
              {quickActions.map((action, index) => {
                const Icon = quickActionIcons[index];
                return (
                  <button
                    key={action.id}
                    type="button"
                    onClick={() => router.push(quickActionRoutes[action.id] ?? "/dashboard")}
                    className="rounded-[1.25rem] border border-surface-border bg-white/[0.03] p-4 text-left transition hover:border-surface-border-strong hover:bg-white/[0.06]"
                  >
                    <div className="flex size-11 items-center justify-center rounded-2xl bg-white/[0.05] text-accent-cyan">
                      <Icon className="size-5" />
                    </div>
                    <p className="mt-4 text-sm font-semibold text-white">{action.label}</p>
                    <p className="mt-1 text-sm text-muted-foreground">
                      {action.description}
                    </p>
                  </button>
                );
              })}
            </CardContent>
          </Card>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Occupancy Overview</CardTitle>
          <CardDescription>
            Live room readiness and occupancy across all hotel floors.
          </CardDescription>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          {occupancy.length ? (
            occupancy.map((room) => (
              <div
                key={room.id}
                className="rounded-[1.35rem] border border-surface-border bg-white/[0.03] p-4"
              >
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-2xl font-bold text-white">{room.roomNumber}</p>
                    <p className="text-sm text-muted-foreground">{room.type}</p>
                  </div>
                  <StatusBadge status={room.status} />
                </div>
                <div className="mt-4 flex items-center justify-between text-sm">
                  <span className="text-muted-foreground">Floor {room.floor}</span>
                  <span className="text-slate-200">{room.beds} beds</span>
                </div>
                {room.guestName ? (
                  <p className="mt-3 text-sm font-semibold text-accent-orange">
                    Guest: {room.guestName}
                  </p>
                ) : null}
              </div>
            ))
          ) : (
            <p className="text-sm text-muted-foreground">No rooms returned from the API.</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
