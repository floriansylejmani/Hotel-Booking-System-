"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Eye, Plus, Search } from "lucide-react";
import { useDeferredValue, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { EmptyState } from "@/components/shared/empty-state";
import { StatusBadge } from "@/components/shared/status-badge";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Modal } from "@/components/ui/modal";
import { SectionHeading } from "@/components/ui/section-heading";
import { Select } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow } from "@/components/ui/table";
import {
  bookingsQueryKey,
  cancelBooking,
  createBooking,
  getBooking,
  getBookings,
  searchBookingsByTerm,
} from "@/features/bookings/api";
import { getRooms } from "@/features/rooms/api";
import { useAuth } from "@/hooks/use-auth";
import { useToast } from "@/hooks/use-toast";
import { formatCompactDate, formatCurrency } from "@/lib/format";
import { hasPermission } from "@/lib/permissions";
import { getApiErrorMessage } from "@/services/api-client";
import type { PaymentMethod } from "@/types/hotel";

const bookingFilters = [
  "all",
  "active",
  "confirmed",
  "checked-out",
  "cancelled",
] as const;

const bookingSchema = z
  .object({
    roomId: z.string().min(1, "Select a room."),
    checkInDate: z.string().min(1, "Check-in date is required."),
    checkOutDate: z.string().min(1, "Check-out date is required."),
    paymentMethod: z.enum(["credit-card", "cash", "check"]),
    guestCount: z.coerce.number().min(1, "Guest count must be at least 1."),
    specialRequests: z.string().max(400, "Keep special requests under 400 characters."),
  })
  .refine((values) => values.checkOutDate > values.checkInDate, {
    message: "Check-out must be after check-in.",
    path: ["checkOutDate"],
  });

type BookingFormValues = z.infer<typeof bookingSchema>;
type BookingFormInput = z.input<typeof bookingSchema>;

export function BookingsWorkspace() {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const { addToast } = useToast();
  const [initialFormValues] = useState<BookingFormInput>(getDefaultBookingFormValues);
  const [selectedFilter, setSelectedFilter] =
    useState<(typeof bookingFilters)[number]>("all");
  const [search, setSearch] = useState("");
  const [selectedBookingId, setSelectedBookingId] = useState<string | null>(null);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const deferredSearch = useDeferredValue(search);
  const canManageBookings = user ? hasPermission(user.role, "bookings.manage") : false;

  const bookingListQuery = useQuery({
    queryKey: [...bookingsQueryKey, selectedFilter, deferredSearch],
    queryFn: () =>
      deferredSearch.trim()
        ? searchBookingsByTerm(deferredSearch, selectedFilter)
        : getBookings({ status: selectedFilter, pageSize: 100 }),
  });

  const roomsQuery = useQuery({
    queryKey: ["rooms", "available-for-booking"],
    queryFn: () => getRooms({ status: "available", pageSize: 100 }),
    enabled: isCreateModalOpen,
  });

  const bookingDetailQuery = useQuery({
    queryKey: ["bookings", "details", selectedBookingId],
    queryFn: () => getBooking(selectedBookingId!),
    enabled: Boolean(selectedBookingId),
  });

  const createMutation = useMutation({
    mutationFn: createBooking,
    onSuccess: async (booking) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: bookingsQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
        queryClient.invalidateQueries({ queryKey: ["notifications"] }),
        queryClient.invalidateQueries({ queryKey: ["rooms"] }),
        queryClient.invalidateQueries({ queryKey: ["checkin-stats"] }),
        queryClient.invalidateQueries({ queryKey: ["invoice"] }),
      ]);
      addToast({
        title: "Booking created",
        description: `${booking.id} was created successfully.`,
        tone: "success",
      });
      setIsCreateModalOpen(false);
      setSelectedBookingId(booking.recordId);
    },
    onError: (error) => {
      addToast({
        title: "Unable to create booking",
        description: getApiErrorMessage(error, "Booking creation failed."),
        tone: "warning",
      });
    },
  });

  const cancelMutation = useMutation({
    mutationFn: cancelBooking,
    onSuccess: async (booking) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: bookingsQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
        queryClient.invalidateQueries({ queryKey: ["notifications"] }),
        queryClient.invalidateQueries({ queryKey: ["rooms"] }),
        queryClient.invalidateQueries({ queryKey: ["checkin-stats"] }),
        queryClient.invalidateQueries({ queryKey: ["invoice"] }),
      ]);
      addToast({
        title: "Booking cancelled",
        description: `${booking.id} was cancelled successfully.`,
        tone: "warning",
      });
      setSelectedBookingId(booking.recordId);
    },
    onError: (error) => {
      addToast({
        title: "Unable to cancel booking",
        description: getApiErrorMessage(error, "Booking cancellation failed."),
        tone: "warning",
      });
    },
  });

  const form = useForm<BookingFormInput, unknown, BookingFormValues>({
    resolver: zodResolver(bookingSchema),
    defaultValues: initialFormValues,
  });

  const bookings = bookingListQuery.data?.items ?? [];
  const selectedBooking =
    bookingDetailQuery.data ??
    bookings.find((booking) => booking.recordId === selectedBookingId) ??
    null;

  const bookableRooms = roomsQuery.data?.items ?? [];
  const canCancelBooking =
    canManageBookings &&
    !!selectedBooking &&
    (selectedBooking.status === "active" || selectedBooking.status === "confirmed");

  const emptyStateDescription = useMemo(() => {
    if (deferredSearch.trim()) {
      return "Try a different guest name, booking code, or room number.";
    }

    return "No bookings were returned for the current filter.";
  }, [deferredSearch]);

  const onCreateBooking = form.handleSubmit(async (values) => {
    await createMutation.mutateAsync({
      roomId: values.roomId,
      checkInDate: values.checkInDate,
      checkOutDate: values.checkOutDate,
      paymentMethod: values.paymentMethod,
      guestCount: values.guestCount,
      specialRequests: values.specialRequests || undefined,
    });
  });

  if (bookingListQuery.isLoading) {
    return <div className="py-16 text-sm text-muted-foreground">Loading bookings...</div>;
  }

  if (bookingListQuery.isError) {
    return (
      <div className="rounded-[1.5rem] border border-accent-red/20 bg-accent-red/10 px-6 py-5 text-sm text-accent-red">
        {getApiErrorMessage(bookingListQuery.error, "Unable to load bookings.")}
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Phase 10"
        title="Bookings"
        description="Search live reservations by guest, booking code, or room number, then inspect details or create and cancel bookings through the backend."
        actions={
          <Button
            onClick={() => {
              form.reset(getDefaultBookingFormValues());
              setIsCreateModalOpen(true);
            }}
            disabled={!canManageBookings}
          >
            <Plus className="size-4" />
            New Booking
          </Button>
        }
      />

      <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
        <div className="flex flex-wrap gap-2">
          {bookingFilters.map((filter) => (
            <button
              key={filter}
              type="button"
              onClick={() => setSelectedFilter(filter)}
              className={`rounded-full border px-4 py-2 text-sm font-semibold transition ${
                selectedFilter === filter
                  ? "border-accent-indigo/35 bg-accent-indigo/16 text-white"
                  : "border-surface-border bg-white/[0.03] text-muted-foreground hover:text-white"
              }`}
            >
              {filter === "all" ? "All Bookings" : filter.replaceAll("-", " ")}
            </button>
          ))}
        </div>

        <div className="relative w-full max-w-md">
          <Search className="pointer-events-none absolute left-4 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search guest, booking code, or room..."
            className="pl-11"
          />
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Reservation register</CardTitle>
          <CardDescription>
            {bookingListQuery.data?.totalCount ?? bookings.length} reservation records
            match the current filters.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {bookings.length ? (
            <Table>
              <TableHead>
                <tr>
                  <TableHeaderCell>ID</TableHeaderCell>
                  <TableHeaderCell>Guest</TableHeaderCell>
                  <TableHeaderCell>Room</TableHeaderCell>
                  <TableHeaderCell>Type</TableHeaderCell>
                  <TableHeaderCell>Check-in</TableHeaderCell>
                  <TableHeaderCell>Check-out</TableHeaderCell>
                  <TableHeaderCell>Amount</TableHeaderCell>
                  <TableHeaderCell>Payment</TableHeaderCell>
                  <TableHeaderCell>Status</TableHeaderCell>
                  <TableHeaderCell />
                </tr>
              </TableHead>
              <TableBody>
                {bookings.map((booking) => (
                  <TableRow key={booking.recordId}>
                    <TableCell className="font-semibold text-[#9ea7ff]">
                      {booking.id}
                    </TableCell>
                    <TableCell className="font-semibold text-white">
                      {booking.guestName}
                    </TableCell>
                    <TableCell>{booking.roomNumber}</TableCell>
                    <TableCell>{booking.roomType}</TableCell>
                    <TableCell>{formatCompactDate(booking.checkIn)}</TableCell>
                    <TableCell>{formatCompactDate(booking.checkOut)}</TableCell>
                    <TableCell className="font-semibold text-accent-green">
                      {formatCurrency(booking.amount)}
                    </TableCell>
                    <TableCell>
                      <Badge variant="neutral" size="sm">
                        {getPaymentMethodLabel(booking.paymentMethod)}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <StatusBadge status={booking.status} />
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="secondary"
                        size="sm"
                        onClick={() => setSelectedBookingId(booking.recordId)}
                      >
                        <Eye className="size-4" />
                        View
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <EmptyState
              icon={Search}
              title="No bookings found"
              description={emptyStateDescription}
            />
          )}
        </CardContent>
      </Card>

      <Modal
        open={!!selectedBookingId}
        onClose={() => setSelectedBookingId(null)}
        title={selectedBooking ? `Booking ${selectedBooking.id}` : "Booking details"}
        description="Reservation details from the live booking API."
        footer={
          <div className="flex justify-end gap-3">
            <Button variant="ghost" onClick={() => setSelectedBookingId(null)}>
              Close
            </Button>
            {canCancelBooking ? (
              <Button
                variant="danger"
                onClick={() =>
                  selectedBooking
                    ? void cancelMutation.mutateAsync(selectedBooking.recordId)
                    : undefined
                }
                disabled={cancelMutation.isPending}
              >
                {cancelMutation.isPending ? "Cancelling..." : "Cancel booking"}
              </Button>
            ) : null}
          </div>
        }
      >
        {bookingDetailQuery.isLoading ? (
          <p className="text-sm text-muted-foreground">Loading booking details...</p>
        ) : selectedBooking ? (
          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Guest details</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <p className="text-sm text-slate-200">
                  <span className="text-muted-foreground">Name:</span>{" "}
                  {selectedBooking.guestName}
                </p>
                <p className="text-sm text-slate-200">
                  <span className="text-muted-foreground">Email:</span>{" "}
                  {selectedBooking.guestEmail ?? "Not returned"}
                </p>
                <p className="text-sm text-slate-200">
                  <span className="text-muted-foreground">Room:</span>{" "}
                  {selectedBooking.roomNumber}
                </p>
                <p className="text-sm text-slate-200">
                  <span className="text-muted-foreground">Type:</span>{" "}
                  {selectedBooking.roomType}
                </p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Booking status</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <StatusBadge status={selectedBooking.status} />
                <p className="text-sm text-slate-200">
                  Check-in: {formatCompactDate(selectedBooking.checkIn)}
                </p>
                <p className="text-sm text-slate-200">
                  Check-out: {formatCompactDate(selectedBooking.checkOut)}
                </p>
                <p className="text-sm text-slate-200">
                  Special request: {selectedBooking.specialRequest ?? "None"}
                </p>
                <p className="text-sm font-semibold text-accent-green">
                  {formatCurrency(selectedBooking.amount)}
                </p>
              </CardContent>
            </Card>
          </div>
        ) : (
          <p className="text-sm text-muted-foreground">Booking details are unavailable.</p>
        )}
      </Modal>

      <Modal
        open={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        title="Create booking"
        description="The current backend associates the new booking with the authenticated user."
        footer={
          <div className="flex justify-end gap-3">
            <Button variant="ghost" onClick={() => setIsCreateModalOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={() => void onCreateBooking()}
              disabled={!canManageBookings || createMutation.isPending}
            >
              {createMutation.isPending ? "Creating..." : "Create booking"}
            </Button>
          </div>
        }
      >
        <div className="grid gap-4 md:grid-cols-2">
          <div className="space-y-2 md:col-span-2">
            <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Room
            </label>
            <Select {...form.register("roomId")}>
              <option value="">Select room</option>
              {bookableRooms.map((room) => (
                <option key={room.id} value={room.id}>
                  Room {room.roomNumber} - {room.type} - {formatCurrency(room.pricePerNight)}
                </option>
              ))}
            </Select>
            {form.formState.errors.roomId ? (
              <p className="text-sm text-accent-red">
                {form.formState.errors.roomId.message}
              </p>
            ) : null}
          </div>
          <div className="space-y-2">
            <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Check-in
            </label>
            <Input type="date" {...form.register("checkInDate")} />
          </div>
          <div className="space-y-2">
            <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Check-out
            </label>
            <Input type="date" {...form.register("checkOutDate")} />
            {form.formState.errors.checkOutDate ? (
              <p className="text-sm text-accent-red">
                {form.formState.errors.checkOutDate.message}
              </p>
            ) : null}
          </div>
          <div className="space-y-2">
            <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Payment method
            </label>
            <Select {...form.register("paymentMethod")}>
              <option value="credit-card">Card</option>
              <option value="cash">Cash</option>
              <option value="check">Check</option>
            </Select>
          </div>
          <div className="space-y-2">
            <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Guest count
            </label>
            <Input type="number" min={1} {...form.register("guestCount")} />
          </div>
        </div>

        <div className="mt-4 space-y-2">
          <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
            Special requests
          </label>
          <Input {...form.register("specialRequests")} />
          {form.formState.errors.specialRequests ? (
            <p className="text-sm text-accent-red">
              {form.formState.errors.specialRequests.message}
            </p>
          ) : null}
          {roomsQuery.isLoading ? (
            <p className="text-xs text-muted-foreground">Loading available rooms...</p>
          ) : null}
        </div>
      </Modal>
    </div>
  );
}

function getPaymentMethodLabel(method: PaymentMethod) {
  switch (method) {
    case "cash":
      return "Cash";
    case "check":
      return "Check";
    default:
      return "Card";
  }
}

function getDefaultBookingFormValues(): BookingFormInput {
  const checkIn = new Date();
  const checkOut = new Date(checkIn);
  checkOut.setDate(checkOut.getDate() + 1);

  return {
    roomId: "",
    checkInDate: checkIn.toISOString().slice(0, 10),
    checkOutDate: checkOut.toISOString().slice(0, 10),
    paymentMethod: "credit-card",
    guestCount: 1,
    specialRequests: "",
  };
}
