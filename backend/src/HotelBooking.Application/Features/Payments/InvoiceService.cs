using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Common.Settings;
using HotelBooking.Application.Features.Payments.DTOs;
using HotelBooking.Domain.Entities;
using HotelBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HotelBooking.Application.Features.Payments;

public sealed class InvoiceService(
    IAppDbContext db,
    IInvoicePdfService invoicePdfService,
    IOptions<TaxSettings> taxOptions) : IInvoiceService
{
    public async Task<InvoiceResponse> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var invoice = await EnsureInvoiceAsync(bookingId, cancellationToken);
        return MapToResponse(invoice);
    }

    public async Task<byte[]> GetPdfByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var invoice = await EnsureInvoiceAsync(bookingId, cancellationToken);
        return invoicePdfService.GeneratePdf(MapToResponse(invoice));
    }

    internal async Task<Invoice> EnsureInvoiceAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await db.Bookings
            .Include(b => b.User)
            .Include(b => b.Room)
            .Include(b => b.RoomCharges)
            .Include(b => b.Invoice)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken)
            ?? throw new NotFoundException("Booking", bookingId);

        if (booking.Status == BookingStatus.Cancelled)
            throw new BadRequestException("Cannot generate invoice for a cancelled booking.");

        if (booking.Invoice is not null)
        {
            return await db.Invoices
                .Include(i => i.Booking).ThenInclude(b => b.User)
                .Include(i => i.Booking).ThenInclude(b => b.Room)
                .Include(i => i.Booking).ThenInclude(b => b.RoomCharges)
                .FirstAsync(i => i.Id == booking.Invoice.Id, cancellationToken);
        }

        var nights = booking.CheckOutDate.DayNumber - booking.CheckInDate.DayNumber;
        var roomStayCost = nights * booking.Room.PricePerNight;
        var extras = booking.RoomCharges.Sum(c => c.Amount);
        var subtotal = roomStayCost + extras;
        var taxAmount = Math.Round(subtotal * taxOptions.Value.Rate, 2);
        var total = subtotal + taxAmount;

        var invoice = new Invoice
        {
            BookingId = booking.Id,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{booking.BookingCode}",
            Subtotal = subtotal,
            TaxRate = taxOptions.Value.Rate,
            TaxAmount = taxAmount,
            TotalAmount = total,
            IssuedAt = DateTime.UtcNow
        };

        db.Invoices.Add(invoice);
        booking.Invoice = invoice;
        booking.TotalAmount = total;
        await db.SaveChangesAsync(cancellationToken);

        return await db.Invoices
            .Include(i => i.Booking).ThenInclude(b => b.User)
            .Include(i => i.Booking).ThenInclude(b => b.Room)
            .Include(i => i.Booking).ThenInclude(b => b.RoomCharges)
            .FirstAsync(i => i.Id == invoice.Id, cancellationToken);
    }

    private static InvoiceResponse MapToResponse(Invoice invoice)
    {
        var booking = invoice.Booking;
        var nights = booking.CheckOutDate.DayNumber - booking.CheckInDate.DayNumber;
        var roomStayTotal = nights * booking.Room.PricePerNight;

        var items = new List<InvoiceItemResponse>
        {
            new($"Room stay ({nights} nights @ {booking.Room.PricePerNight:F2})", roomStayTotal, roomStayTotal)
        };

        items.AddRange(booking.RoomCharges.Select(charge =>
            new InvoiceItemResponse(charge.Description, charge.Amount, charge.Amount)));

        return new InvoiceResponse(
            invoice.Id,
            invoice.InvoiceNumber,
            booking.Id,
            booking.User.FullName,
            booking.User.Email,
            booking.Room.RoomNumber,
            booking.Room.Type.ToString(),
            booking.CheckInDate,
            booking.CheckOutDate,
            items,
            invoice.Subtotal,
            invoice.TaxAmount,
            invoice.TotalAmount,
            invoice.IssuedAt);
    }
}
