using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Persistence.Configurations;

public class RoomChargeConfiguration : IEntityTypeConfiguration<RoomCharge>
{
    public void Configure(EntityTypeBuilder<RoomCharge> builder)
    {
        builder.HasKey(rc => rc.Id);
        builder.Property(rc => rc.Description).IsRequired().HasMaxLength(200);
        builder.Property(rc => rc.Amount).HasPrecision(10, 2);
        
        builder.HasIndex(rc => rc.BookingId);

        builder.HasOne(rc => rc.Booking)
            .WithMany(b => b.RoomCharges)
            .HasForeignKey(rc => rc.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
