namespace HotelBooking.Application.Features.Payments.DTOs;

public sealed record InvoiceResponse(
    Guid InvoiceId,
    string InvoiceNumber,
    Guid BookingId,
    string GuestName,
    string GuestEmail,
    string RoomNumber,
    string RoomType,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    List<InvoiceItemResponse> Items,
    decimal Subtotal,
    decimal TaxAmount,
    decimal TotalAmount,
    DateTime IssuedAt);

public sealed record InvoiceItemResponse(
    string Description,
    decimal Amount,
    decimal Total);

public sealed record ProcessPaymentRequest(
    Guid BookingId,
    string PaymentMethod,
    decimal Amount);

public sealed record PaymentResponse(
    Guid PaymentId,
    Guid InvoiceId,
    string PaymentMethod,
    decimal Amount,
    DateTime? PaidAt,
    string Status,
    string Message);
