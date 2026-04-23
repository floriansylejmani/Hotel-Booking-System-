using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Payments.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HotelBooking.Infrastructure.Services;

public class InvoicePdfService : IInvoicePdfService
{
    public byte[] GeneratePdf(InvoiceResponse invoice)
    {
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);

                page.Header().Element(ComposeHeader);
                page.Content().Element(c => ComposeContent(c, invoice));
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();

        return pdf;
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeColumn().Text("INVOICE").FontSize(28).Bold();
            row.RelativeColumn().AlignRight().Column(col =>
            {
                col.Item().Text("Hotel Booking System").FontSize(12).Bold();
                col.Item().Text("123 Hotel Street, City");
                col.Item().Text("contact@hotelbooking.com");
            });
        });
    }

    private void ComposeContent(IContainer container, InvoiceResponse invoice)
    {
        container.PaddingVertical(0.5f, Unit.Centimetre);

        container.Row(row =>
        {
            row.RelativeColumn().Column(col =>
            {
                col.Item().Text("Invoice Number").FontSize(9).Bold().FontColor("#999999");
                col.Item().Text(invoice.InvoiceNumber).FontSize(11);

                col.Item().PaddingTop(0.5f, Unit.Centimetre);
                col.Item().Text("Issue Date").FontSize(9).Bold().FontColor("#999999");
                col.Item().Text(invoice.IssuedAt.ToString("yyyy-MM-dd"));
            });

            row.RelativeColumn().AlignRight().Column(col =>
            {
                col.Item().Text("Guest").FontSize(9).Bold().FontColor("#999999");
                col.Item().Text(invoice.GuestName).FontSize(11);
                col.Item().Text(invoice.GuestEmail).FontSize(9);

                col.Item().PaddingTop(0.5f, Unit.Centimetre);
                col.Item().Text("Room").FontSize(9).Bold().FontColor("#999999");
                col.Item().Text($"{invoice.RoomNumber} ({invoice.RoomType})").FontSize(11);
            });
        });

        container.PaddingVertical(0.3f, Unit.Centimetre);

        container.Row(row =>
        {
            row.RelativeColumn().Column(col =>
            {
                col.Item().Text("Check-in").FontSize(9).Bold().FontColor("#999999");
                col.Item().Text(invoice.CheckInDate.ToString("yyyy-MM-dd")).FontSize(10);

                col.Item().PaddingTop(0.3f, Unit.Centimetre);
                col.Item().Text("Check-out").FontSize(9).Bold().FontColor("#999999");
                col.Item().Text(invoice.CheckOutDate.ToString("yyyy-MM-dd")).FontSize(10);
            });

            row.RelativeColumn().AlignRight().Column(col =>
            {
                col.Item().Text("Nights").FontSize(9).Bold().FontColor("#999999");
                col.Item().Text((invoice.CheckOutDate.DayNumber - invoice.CheckInDate.DayNumber).ToString()).FontSize(10);

                col.Item().PaddingTop(0.3f, Unit.Centimetre);
                col.Item().Text("Booking").FontSize(9).Bold().FontColor("#999999");
                col.Item().Text(invoice.BookingId.ToString("D").Substring(0, 8).ToUpper()).FontSize(10);
            });
        });

        container.PaddingVertical(0.5f, Unit.Centimetre);

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.ConstantColumn(100);
            });

            table.Header(header =>
            {
                header.Cell().Background("#f0f0f0").Padding(0.3f, Unit.Centimetre).Text("Description").FontSize(9).Bold();
                header.Cell().Background("#f0f0f0").Padding(0.3f, Unit.Centimetre).AlignRight().Text("Amount").FontSize(9).Bold();
            });

            foreach (var item in invoice.Items)
            {
                table.Cell().Padding(0.3f, Unit.Centimetre).Text(item.Description).FontSize(9);
                table.Cell().Padding(0.3f, Unit.Centimetre).AlignRight().Text($"${item.Total:F2}").FontSize(9);
            }
        });

        container.PaddingVertical(0.5f, Unit.Centimetre);

        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeColumn(2);
                row.RelativeColumn().Text("Subtotal:").AlignRight().FontSize(10);
                row.ConstantColumn(100).Text($"${invoice.Subtotal:F2}").AlignRight().Bold().FontSize(10);
            });

            col.Item().Row(row =>
            {
                row.RelativeColumn(2);
                row.RelativeColumn().Text("Tax (12%):").AlignRight().FontSize(10);
                row.ConstantColumn(100).Text($"${invoice.TaxAmount:F2}").AlignRight().Bold().FontSize(10);
            });

            col.Item().PaddingTop(0.3f, Unit.Centimetre).BorderTop(1).Row(row =>
            {
                row.RelativeColumn(2);
                row.RelativeColumn().Text("Total:").AlignRight().FontSize(12).Bold();
                row.ConstantColumn(100).Text($"${invoice.TotalAmount:F2}").AlignRight().Bold().FontSize(12);
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text("Thank you for your business!").FontSize(9).FontColor("#999999");
    }
}
