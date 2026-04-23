"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { DoorClosed, DoorOpen, Search } from "lucide-react";
import { useDeferredValue, useState } from "react";
import { EmptyState } from "@/components/shared/empty-state";
import { StatusBadge } from "@/components/shared/status-badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { SectionHeading } from "@/components/ui/section-heading";
import {
  checkInBooking,
  checkOutBooking,
  checkinStatsQueryKey,
  getCheckinStats,
  searchCheckinBooking,
} from "@/features/checkin/api";
import { useAuth } from "@/hooks/use-auth";
import { useToast } from "@/hooks/use-toast";
import { formatCompactDate, formatCurrency } from "@/lib/format";
import { hasPermission } from "@/lib/permissions";
import { getApiErrorMessage } from "@/services/api-client";

export function CheckinWorkspace() {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const { addToast } = useToast();
  const canManageCheckins = user ? hasPermission(user.role, "checkin.manage") : false;
  const [searchQuery, setSearchQuery] = useState("");
  const deferredQuery = useDeferredValue(searchQuery);

  const statsQuery = useQuery({
    queryKey: checkinStatsQueryKey,
    queryFn: getCheckinStats,
  });
  const searchQueryResult = useQuery({
    queryKey: ["checkin-search", deferredQuery],
    queryFn: () => searchCheckinBooking(deferredQuery),
    enabled: deferredQuery.trim().length > 0,
  });

  const checkInMutation = useMutation({
    mutationFn: checkInBooking,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: checkinStatsQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["checkin-search"] }),
        queryClient.invalidateQueries({ queryKey: ["bookings"] }),
        queryClient.invalidateQueries({ queryKey: ["rooms"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
        queryClient.invalidateQueries({ queryKey: ["notifications"] }),
      ]);
      addToast({
        title: "Guest checked in",
        description: "The booking and room statuses were updated.",
        tone: "success",
      });
    },
    onError: (error) => {
      addToast({
        title: "Unable to check in guest",
        description: getApiErrorMessage(error, "Check-in failed."),
        tone: "warning",
      });
    },
  });

  const checkOutMutation = useMutation({
    mutationFn: checkOutBooking,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: checkinStatsQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["checkin-search"] }),
        queryClient.invalidateQueries({ queryKey: ["bookings"] }),
        queryClient.invalidateQueries({ queryKey: ["rooms"] }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
        queryClient.invalidateQueries({ queryKey: ["notifications"] }),
      ]);
      addToast({
        title: "Guest checked out",
        description: "The stay was closed successfully.",
        tone: "success",
      });
    },
    onError: (error) => {
      addToast({
        title: "Unable to check out guest",
        description: getApiErrorMessage(error, "Check-out failed."),
        tone: "warning",
      });
    },
  });

  const stats = statsQuery.data;
  const booking = searchQueryResult.data ?? null;

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Phase 10"
        title="Check-in / Check-out"
        description="Search by guest, booking code, or room number, then transition stays through the live arrival and departure endpoints."
      />

      <Card>
        <CardHeader>
          <CardTitle>Find booking</CardTitle>
          <CardDescription>
            Search by guest name, booking code, or room number.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="relative max-w-xl">
            <Search className="pointer-events-none absolute left-4 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={searchQuery}
              onChange={(event) => setSearchQuery(event.target.value)}
              placeholder="Search by guest name, booking code, or room number..."
              className="pl-11"
            />
          </div>
        </CardContent>
      </Card>

      <div className="mx-auto grid max-w-3xl gap-4 md:grid-cols-3">
        <Card>
          <CardContent className="pt-6 text-center">
            <DoorOpen className="mx-auto size-8 text-accent-green" />
            <p className="mt-3 text-4xl font-bold text-accent-green">
              {statsQuery.isLoading ? "-" : stats?.todayCheckins ?? 0}
            </p>
            <p className="mt-2 text-sm text-muted-foreground">Today&apos;s check-ins</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <DoorClosed className="mx-auto size-8 text-accent-red" />
            <p className="mt-3 text-4xl font-bold text-accent-red">
              {statsQuery.isLoading ? "-" : stats?.todayCheckouts ?? 0}
            </p>
            <p className="mt-2 text-sm text-muted-foreground">Today&apos;s check-outs</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <DoorOpen className="mx-auto size-8 text-[#9ea7ff]" />
            <p className="mt-3 text-4xl font-bold text-[#9ea7ff]">
              {statsQuery.isLoading ? "-" : stats?.activeStays ?? 0}
            </p>
            <p className="mt-2 text-sm text-muted-foreground">Active stays</p>
          </CardContent>
        </Card>
      </div>

      {!deferredQuery.trim() ? (
        <EmptyState
          icon={Search}
          title="Search for a booking above"
          description="Enter a guest name, booking code, or room number to load a live booking result."
        />
      ) : null}

      {searchQueryResult.isError ? (
        <div className="rounded-[1.5rem] border border-accent-red/20 bg-accent-red/10 px-6 py-5 text-sm text-accent-red">
          {getApiErrorMessage(searchQueryResult.error, "Unable to search for booking.")}
        </div>
      ) : null}

      {deferredQuery.trim() && !searchQueryResult.isLoading && !booking && !searchQueryResult.isError ? (
        <EmptyState
          icon={Search}
          title="No booking found"
          description="Try a different guest name, booking code, or room number."
        />
      ) : null}

      {booking ? (
        <Card>
          <CardHeader className="flex flex-row items-start justify-between gap-4">
            <div>
              <CardTitle>{booking.guestName}</CardTitle>
              <CardDescription>
                {booking.id} · Room {booking.roomNumber} · {booking.roomType}
              </CardDescription>
            </div>
            <StatusBadge status={booking.status} />
          </CardHeader>
          <CardContent className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_auto]">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="rounded-[1.25rem] border border-surface-border bg-white/[0.03] p-4">
                <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                  Stay window
                </p>
                <p className="mt-3 text-sm text-slate-200">
                  Check-in: {formatCompactDate(booking.checkIn)}
                </p>
                <p className="mt-2 text-sm text-slate-200">
                  Check-out: {formatCompactDate(booking.checkOut)}
                </p>
                {booking.actualCheckIn ? (
                  <p className="mt-2 text-sm text-slate-200">
                    Actual check-in: {formatCompactDate(booking.actualCheckIn)}
                  </p>
                ) : null}
                {booking.actualCheckOut ? (
                  <p className="mt-2 text-sm text-slate-200">
                    Actual check-out: {formatCompactDate(booking.actualCheckOut)}
                  </p>
                ) : null}
              </div>
              <div className="rounded-[1.25rem] border border-surface-border bg-white/[0.03] p-4">
                <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                  Billing snapshot
                </p>
                <p className="mt-3 text-2xl font-bold text-accent-green">
                  {formatCurrency(booking.amount)}
                </p>
                <p className="mt-2 text-sm text-muted-foreground">
                  Special request: {booking.specialRequest ?? "None"}
                </p>
              </div>
            </div>

            <div className="flex flex-wrap items-center gap-3">
              {booking.status === "confirmed" ? (
                <Button
                  onClick={() => void checkInMutation.mutateAsync(booking.recordId)}
                  disabled={!canManageCheckins || checkInMutation.isPending}
                >
                  {checkInMutation.isPending ? "Checking in..." : "Check in guest"}
                </Button>
              ) : null}
              {booking.status === "active" ? (
                <Button
                  variant="secondary"
                  onClick={() => void checkOutMutation.mutateAsync(booking.recordId)}
                  disabled={!canManageCheckins || checkOutMutation.isPending}
                >
                  {checkOutMutation.isPending ? "Checking out..." : "Check out guest"}
                </Button>
              ) : null}
            </div>
          </CardContent>
        </Card>
      ) : null}
    </div>
  );
}
