using HotelBooking.Domain.Common;
using HotelBooking.Domain.Enums;

namespace HotelBooking.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string Method { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public DateTime? PaidAt { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
