using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Persistence.Configurations;

public class HousekeepingTaskConfiguration : IEntityTypeConfiguration<HousekeepingTask>
{
    public void Configure(EntityTypeBuilder<HousekeepingTask> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Notes).HasMaxLength(500);
        builder.Property(h => h.Status).IsRequired();
        builder.HasIndex(h => new { h.RoomId, h.CreatedAt });
        builder.HasIndex(h => new { h.AssignedToUserId, h.Status });

        builder.HasOne(h => h.Room)
            .WithMany(r => r.HousekeepingTasks)
            .HasForeignKey(h => h.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.AssignedTo)
            .WithMany(u => u.HousekeepingTasks)
            .HasForeignKey(h => h.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
