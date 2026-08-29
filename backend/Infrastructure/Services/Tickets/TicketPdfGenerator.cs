using Application.DTOs.Tickets;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services.Tickets;

internal static class TicketPdfGenerator {
    static TicketPdfGenerator() {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] Generate(TicketDto ticket) {
        var qrPng = CreateQrPng(ticket.QrCode);

        return Document.Create(container => {
            container.Page(page => {
                page.Size(PageSizes.A6);
                page.Margin(24);
                page.PageColor(Colors.White);

                page.Content().Column(column => {
                    column.Spacing(8);

                    column.Item().Text("Event ticket").FontSize(10).FontColor(Colors.Grey.Darken1);
                    column.Item().Text(ticket.EventTitle).FontSize(18).Bold();
                    column.Item().Text($"{ticket.EventDate:yyyy-MM-dd}  {ticket.EventStartTime:hh\\:mm}");
                    column.Item().Text(ticket.EventLocation);

                    column.Item().PaddingVertical(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    column.Item().Text($"Ticket  {ticket.TicketNumber}").SemiBold();
                    column.Item().Text($"Order   {ticket.OrderNumber}");
                    column.Item().Text($"Buyer   {ticket.BuyerName}");
                    column.Item().Text($"Email   {ticket.BuyerEmail}");
                    column.Item().Text($"Price   {ticket.Price:0.00}");

                    if (!string.IsNullOrWhiteSpace(ticket.SeatInfo)) {
                        column.Item().Text($"Seat    {ticket.SeatInfo}");
                    }

                    column.Item().AlignCenter().Width(140).Image(qrPng);
                    column.Item().AlignCenter().Text(ticket.QrCode).FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf();
    }

    private static byte[] CreateQrPng(string payload) {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(data);
        return qrCode.GetGraphic(8);
    }
}
