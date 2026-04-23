using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelBooking.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Invoices_Subtotal_NonNegative", "\"Subtotal\" >= 0");
            table.HasCheckConstraint("CK_Invoices_TaxAmount_NonNegative", "\"TaxAmount\" >= 0");
            table.HasCheckConstraint("CK_Invoices_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
            table.HasCheckConstraint("CK_Invoices_TaxRate_NonNegative", "\"TaxRate\" >= 0");
        });

        builder.HasKey(i => i.Id);
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.Property(i => i.Subtotal).HasPrecision(10, 2);
        builder.Property(i => i.TaxAmount).HasPrecision(10, 2);
        builder.Property(i => i.TotalAmount).HasPrecision(10, 2);
        builder.Property(i => i.TaxRate).HasPrecision(5, 4);

        builder.HasOne(i => i.Booking)
            .WithOne(b => b.Invoice)
            .HasForeignKey<Invoice>(i => i.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
