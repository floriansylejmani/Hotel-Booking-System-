import { apiClient } from "@/services/api-client";
import { getBookings, searchBookingsByTerm } from "@/features/bookings/api";

type CheckInOutResponseDto = {
  bookingId: string;
  bookingCode: string;
  bookingStatus: string;
  roomNumber: string;
  roomStatus: string;
  timestamp: string;
  message: string;
};

export const checkinStatsQueryKey = ["checkin-stats"] as const;

export async function getCheckinStats() {
  const bookings = await getBookings({ pageSize: 100 });
  const today = new Date().toISOString().slice(0, 10);

  return {
    todayCheckins: bookings.items.filter((booking) => booking.checkIn === today).length,
    todayCheckouts: bookings.items.filter((booking) => booking.checkOut === today).length,
    activeStays: bookings.items.filter((booking) => booking.status === "active").length,
  };
}

export async function searchCheckinBooking(term: string) {
  const result = await searchBookingsByTerm(term);
  return result.items[0] ?? null;
}

export async function checkInBooking(recordId: string) {
  return apiClient<CheckInOutResponseDto>(`/checkin/${recordId}`, {
    method: "PUT",
  });
}

export async function checkOutBooking(recordId: string) {
  return apiClient<CheckInOutResponseDto>(`/checkout/${recordId}`, {
    method: "PUT",
  });
}
