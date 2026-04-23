import { toPaymentMethodApi, toRoomType } from "@/lib/hotel-mappers";
import { apiClient } from "@/services/api-client";
import { getBookings } from "@/features/bookings/api";
import type { Invoice, PaymentMethod } from "@/types/hotel";

type InvoiceResponseDto = {
  invoiceId: string;
  invoiceNumber: string;
  bookingId: string;
  guestName: string;
  guestEmail: string;
  roomNumber: string;
  roomType: string;
  checkInDate: string;
  checkOutDate: string;
  items: Array<{
    description: string;
    amount: number;
    total: number;
  }>;
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  issuedAt: string;
};

type PaymentResponseDto = {
  paymentId: string;
  invoiceId: string;
  paymentMethod: string;
  amount: number;
  paidAt?: string | null;
  status: string;
  message: string;
};

export const invoiceQueryKey = ["invoice"] as const;

function mapInvoice(dto: InvoiceResponseDto): Invoice {
  return {
    invoiceId: dto.invoiceId,
    invoiceNumber: dto.invoiceNumber,
    issuedDate: dto.issuedAt,
    guestName: dto.guestName,
    bookingId: dto.bookingId,
    bookingRecordId: dto.bookingId,
    roomNumber: dto.roomNumber,
    roomType: toRoomType(dto.roomType),
    checkIn: dto.checkInDate,
    checkOut: dto.checkOutDate,
    hotelName: "LuxeStay Hotel",
    hotelAddress: "123 Grand Avenue\nBudapest 1051\nHungary",
    items: dto.items.map((item, index) => ({
      id: `${dto.invoiceId}-${index}`,
      description: item.description,
      rateLabel: item.amount.toFixed(2),
      total: item.total,
    })),
    subtotal: dto.subtotal,
    tax: dto.taxAmount,
    total: dto.totalAmount,
  };
}

export async function getDefaultInvoice(bookingId?: string) {
  let selectedBookingId = bookingId;

  if (!selectedBookingId) {
    const bookings = await getBookings({ pageSize: 100 });
    selectedBookingId =
      bookings.items.find((booking) => booking.status !== "cancelled")?.recordId;
  }

  if (!selectedBookingId) {
    return null;
  }

  const invoice = await apiClient<InvoiceResponseDto>(`/invoices/${selectedBookingId}`);
  return mapInvoice(invoice);
}

export async function downloadInvoicePdf(bookingId: string) {
  return apiClient<Blob>(`/invoices/${bookingId}/pdf`, {
    responseType: "blob",
  });
}

export async function processPayment(
  bookingId: string,
  paymentMethod: PaymentMethod,
  amount: number,
) {
  return apiClient<PaymentResponseDto>("/payments", {
    method: "POST",
    body: {
      bookingId,
      paymentMethod: toPaymentMethodApi(paymentMethod),
      amount,
    },
  });
}
