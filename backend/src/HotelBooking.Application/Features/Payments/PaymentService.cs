using FluentValidation;
using HotelBooking.Application.Common.Events;
using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Notifications;
using HotelBooking.Application.Features.Payments.DTOs;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Application.Features.Payments;

public sealed class PaymentService(
    IAppDbContext db,
    IInvoiceService invoiceService,
    IValidator<ProcessPaymentRequest> validator,
    INotificationService notificationService,
    IPublisher publisher,
    IAuditService auditService,
    ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<PaymentResponse> ProcessAsync(
        ProcessPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var booking = await db.Bookings
            .Include(b => b.Invoice)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken)
            ?? throw new NotFoundException("Booking", request.BookingId);

        if (booking.Status == BookingStatus.Cancelled)
            throw new BadRequestException("Cannot process payment for a cancelled booking.");

        var invoice = await invoiceService.GetByBookingIdAsync(request.BookingId, cancellationToken);
        var totalPaid = await db.Payments
            .Where(p => p.InvoiceId == invoice.InvoiceId && p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount, cancellationToken);

        var remaining = invoice.TotalAmount - totalPaid;
        if (request.Amount > remaining)
            throw new BadRequestException($"Payment amount exceeds remaining balance ({remaining:F2}).");

        var payment = new Payment
        {
            InvoiceId = invoice.InvoiceId,
            Method = request.PaymentMethod,
            Amount = request.Amount,
            Status = PaymentStatus.Completed,
            PaidAt = DateTime.UtcNow
        };

        db.Payments.Add(payment);
        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Payment", payment.Id, "Processed", null,
            $"BookingId={booking.Id} Invoice={invoice.InvoiceNumber} Amount={payment.Amount:C}",
            cancellationToken);

        await notificationService.CreateAsync(
            booking.UserId,
            "Payment Received",
            $"Payment of ${payment.Amount:F2} received for invoice {invoice.InvoiceNumber}.",
            NotificationType.PaymentCompleted,
            cancellationToken);

        await publisher.Publish(new PaymentCompletedEvent(
            payment.Id,
            booking.Id,
            booking.User.Email,
            booking.User.FullName,
            payment.Amount,
            invoice.InvoiceNumber), cancellationToken);

        logger.LogInformation(
            "Payment processed. PaymentId={PaymentId} BookingId={BookingId} InvoiceId={InvoiceId} Amount={Amount}",
            payment.Id,
            booking.Id,
            payment.InvoiceId,
            payment.Amount);

        return new PaymentResponse(
            payment.Id,
            payment.InvoiceId,
            payment.Method,
            payment.Amount,
            payment.PaidAt,
            payment.Status.ToString(),
            "Payment processed successfully.");
    }
}
