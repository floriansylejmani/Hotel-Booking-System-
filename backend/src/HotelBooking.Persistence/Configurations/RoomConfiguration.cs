using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Rooms_FloorNumber_NonNegative", "\"FloorNumber\" >= 0");
            table.HasCheckConstraint("CK_Rooms_BedCount_Positive", "\"BedCount\" >= 1");
            table.HasCheckConstraint("CK_Rooms_PricePerNight_Positive", "\"PricePerNight\" > 0");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomNumber)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(r => r.RoomNumber).IsUnique();
        builder.HasIndex(r => new { r.Status, r.Type });

        builder.Property(r => r.FloorNumber).IsRequired();
        builder.Property(r => r.BedCount).IsRequired();
        builder.Property(r => r.PricePerNight).HasPrecision(10, 2).IsRequired();
        builder.Property(r => r.Type).IsRequired();
        builder.Property(r => r.Status).IsRequired();

        builder.Property(r => r.Amenities)
            .HasColumnType("text[]")
            .IsRequired();
    }
}
