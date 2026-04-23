using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Bookings_DateRange", "\"CheckOutDate\" > \"CheckInDate\"");
            table.HasCheckConstraint("CK_Bookings_GuestCount_Positive", "\"GuestCount\" > 0");
            table.HasCheckConstraint("CK_Bookings_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
        });

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BookingCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(b => b.BookingCode).IsUnique();

        builder.Property(b => b.PaymentMethod).IsRequired();
        builder.Property(b => b.Status).IsRequired();
        builder.Property(b => b.TotalAmount).HasPrecision(10, 2);
        builder.Property(b => b.SpecialRequests).HasMaxLength(500);

        builder.HasIndex(b => new { b.RoomId, b.CheckInDate, b.CheckOutDate });
        builder.HasIndex(b => new { b.UserId, b.Status });
        builder.HasIndex(b => b.Status);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Invoice)
            .WithOne(i => i.Booking)
            .HasForeignKey<Invoice>(i => i.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
