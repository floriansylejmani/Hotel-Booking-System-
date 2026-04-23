import { toBookingStatus, toPaymentMethod, toPaymentMethodApi, toRoomType } from "@/lib/hotel-mappers";
import { apiClient, toQueryString } from "@/services/api-client";
import type { Booking, BookingStatus, PaymentMethod } from "@/types/hotel";

type BookingResponseDto = {
  id: string;
  bookingCode: string;
  userId: string;
  guestName: string;
  guestEmail: string;
  roomId: string;
  roomNumber: string;
  roomType: string;
  checkInDate: string;
  checkOutDate: string;
  totalAmount: number;
  paymentMethod: string;
  status: string;
  nights: number;
  createdAt: string;
};

type BookingDetailsResponseDto = BookingResponseDto & {
  guestCount: number;
  specialRequests?: string | null;
  actualCheckIn?: string | null;
  actualCheckOut?: string | null;
};

type BookingsPagedResultDto = {
  items: BookingResponseDto[];
  totalCount: number;
  page: number;
  pageSize: number;
};

type CreateBookingInput = {
  roomId: string;
  checkInDate: string;
  checkOutDate: string;
  paymentMethod: PaymentMethod;
  guestCount?: number;
  specialRequests?: string;
};

export const bookingsQueryKey = ["bookings"] as const;

function mapBooking(dto: BookingResponseDto | BookingDetailsResponseDto): Booking {
  return {
    id: dto.bookingCode,
    recordId: dto.id,
    userId: dto.userId,
    roomId: dto.roomId,
    guestName: dto.guestName,
    guestEmail: dto.guestEmail,
    roomNumber: dto.roomNumber,
    roomType: toRoomType(dto.roomType),
    checkIn: dto.checkInDate,
    checkOut: dto.checkOutDate,
    amount: dto.totalAmount,
    paymentMethod: toPaymentMethod(dto.paymentMethod),
    status: toBookingStatus(dto.status),
    nights: dto.nights,
    createdAt: dto.createdAt,
    specialRequest:
      "specialRequests" in dto ? (dto.specialRequests ?? undefined) : undefined,
    actualCheckIn:
      "actualCheckIn" in dto ? (dto.actualCheckIn ?? undefined) : undefined,
    actualCheckOut:
      "actualCheckOut" in dto ? (dto.actualCheckOut ?? undefined) : undefined,
  };
}

export async function getBookings(params?: {
  status?: BookingStatus | "all";
  page?: number;
  pageSize?: number;
}) {
  const response = await apiClient<BookingsPagedResultDto>(
    `/bookings${toQueryString({
      status: toBookingStatusApi(params?.status),
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 100,
    })}`,
  );

  return {
    items: response.items.map(mapBooking),
    totalCount: response.totalCount,
    page: response.page,
    pageSize: response.pageSize,
  };
}

export async function searchBookings(params: {
  status?: BookingStatus | "all";
  guestName?: string;
  bookingCode?: string;
  roomNumber?: string;
  page?: number;
  pageSize?: number;
}) {
  const response = await apiClient<BookingsPagedResultDto>(
    `/bookings/search${toQueryString({
      status: toBookingStatusApi(params.status),
      guestName: params.guestName,
      bookingCode: params.bookingCode?.toUpperCase(),
      roomNumber: params.roomNumber?.toUpperCase(),
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 100,
    })}`,
  );

  return {
    items: response.items.map(mapBooking),
    totalCount: response.totalCount,
    page: response.page,
    pageSize: response.pageSize,
  };
}

export async function searchBookingsByTerm(
  term: string,
  status?: BookingStatus | "all",
) {
  const trimmed = term.trim();
  if (!trimmed) {
    return getBookings({ status });
  }

  const searches: Array<ReturnType<typeof searchBookings>> = [];

  if (/^BK-\d+/i.test(trimmed)) {
    searches.push(searchBookings({ bookingCode: trimmed, status }));
  } else if (/^\d+$/i.test(trimmed)) {
    searches.push(searchBookings({ roomNumber: trimmed, status }));
    searches.push(searchBookings({ bookingCode: trimmed, status }));
  } else {
    searches.push(searchBookings({ guestName: trimmed, status }));
    searches.push(searchBookings({ bookingCode: trimmed, status }));
    searches.push(searchBookings({ roomNumber: trimmed, status }));
  }

  const results = await Promise.allSettled(searches);
  const map = new Map<string, Booking>();

  results.forEach((result) => {
    if (result.status !== "fulfilled") {
      return;
    }

    result.value.items.forEach((booking) => {
      map.set(booking.recordId, booking);
    });
  });

  const items = Array.from(map.values()).sort((left, right) =>
    right.checkIn.localeCompare(left.checkIn),
  );

  return {
    items,
    totalCount: items.length,
    page: 1,
    pageSize: items.length || 1,
  };
}

export async function getBooking(recordId: string) {
  return mapBooking(await apiClient<BookingDetailsResponseDto>(`/bookings/${recordId}`));
}

export async function createBooking(input: CreateBookingInput) {
  return mapBooking(
    await apiClient<BookingResponseDto>("/bookings", {
      method: "POST",
      body: {
        roomId: input.roomId,
        checkInDate: input.checkInDate,
        checkOutDate: input.checkOutDate,
        paymentMethod: toPaymentMethodEnum(input.paymentMethod),
        guestCount: input.guestCount ?? 1,
        specialRequests: input.specialRequests,
      },
    }),
  );
}

export async function cancelBooking(recordId: string) {
  return mapBooking(
    await apiClient<BookingResponseDto>(`/bookings/${recordId}/cancel`, {
      method: "PUT",
    }),
  );
}

function toBookingStatusApi(status?: BookingStatus | "all") {
  switch (status) {
    case "active":
      return "Active";
    case "checked-out":
      return "CheckedOut";
    case "cancelled":
      return "Cancelled";
    case "confirmed":
      return "Confirmed";
    default:
      return undefined;
  }
}

function toPaymentMethodEnum(method: PaymentMethod) {
  switch (toPaymentMethodApi(method)) {
    case "Cash":
      return 2;
    case "Check":
      return 3;
    default:
      return 1;
  }
}
