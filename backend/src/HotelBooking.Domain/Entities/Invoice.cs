using HotelBooking.Domain.Common;

namespace HotelBooking.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid BookingId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxRate { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public Booking Booking { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = [];
}
