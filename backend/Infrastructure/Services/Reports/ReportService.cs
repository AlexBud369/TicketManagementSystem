using System.Globalization;
using System.Text;
using Application.DTOs.Reports;
using Application.Interfaces;
using ClosedXML.Excel;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Reports;

public sealed class ReportService : IReportService {
    private readonly AppDbContext _dbContext;

    public ReportService(AppDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<FileExportDto> ExportAsync(
        ReportType type,
        ReportFormat format,
        CancellationToken cancellationToken = default) {
        var table = type switch {
            ReportType.Sales => await BuildSalesAsync(cancellationToken),
            ReportType.Users => await BuildUsersAsync(cancellationToken),
            ReportType.Orders => await BuildOrdersAsync(cancellationToken),
            ReportType.Revenue => await BuildRevenueAsync(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var baseName = $"{type.ToString().ToLowerInvariant()}-{stamp}";

        return format == ReportFormat.Excel
            ? ToExcel(table, baseName)
            : ToCsv(table, baseName);
    }

    private async Task<ReportTable> BuildSalesAsync(CancellationToken cancellationToken) {
        var rows = await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.Status == OrderStatus.Paid)
            .OrderByDescending(order => order.CreatedAt)
            .Select(order => new[] {
                order.OrderNumber,
                order.EventEntity != null ? order.EventEntity.Title : string.Empty,
                order.User != null ? order.User.Email : string.Empty,
                order.Quantity.ToString(CultureInfo.InvariantCulture),
                order.Total.ToString("0.00", CultureInfo.InvariantCulture),
                order.CreatedAt.ToString("u", CultureInfo.InvariantCulture)
            })
            .ToListAsync(cancellationToken);

        return new ReportTable(
            ["OrderNumber", "Event", "BuyerEmail", "Quantity", "Total", "PaidAt"],
            rows);
    }

    private async Task<ReportTable> BuildUsersAsync(CancellationToken cancellationToken) {
        var rows = await _dbContext.AppUsers
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAt)
            .Select(user => new[] {
                user.Id.ToString(),
                user.FirstName,
                user.LastName,
                user.Email,
                user.Status.ToString(),
                user.CreatedAt.ToString("u", CultureInfo.InvariantCulture)
            })
            .ToListAsync(cancellationToken);

        return new ReportTable(
            ["Id", "FirstName", "LastName", "Email", "Status", "CreatedAt"],
            rows);
    }

    private async Task<ReportTable> BuildOrdersAsync(CancellationToken cancellationToken) {
        var rows = await _dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .Select(order => new[] {
                order.OrderNumber,
                order.EventEntity != null ? order.EventEntity.Title : string.Empty,
                order.User != null ? order.User.Email : string.Empty,
                order.Quantity.ToString(CultureInfo.InvariantCulture),
                order.SubTotal.ToString("0.00", CultureInfo.InvariantCulture),
                order.ServiceFee.ToString("0.00", CultureInfo.InvariantCulture),
                order.Total.ToString("0.00", CultureInfo.InvariantCulture),
                order.Status.ToString(),
                order.CreatedAt.ToString("u", CultureInfo.InvariantCulture)
            })
            .ToListAsync(cancellationToken);

        return new ReportTable(
            ["OrderNumber", "Event", "BuyerEmail", "Quantity", "SubTotal", "ServiceFee", "Total", "Status", "CreatedAt"],
            rows);
    }

    private async Task<ReportTable> BuildRevenueAsync(CancellationToken cancellationToken) {
        var rows = await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.Status == OrderStatus.Paid)
            .GroupBy(order => new {
                order.EventId,
                Title = order.EventEntity != null ? order.EventEntity.Title : "Unknown"
            })
            .Select(group => new[] {
                group.Key.Title,
                group.Sum(order => order.Quantity).ToString(CultureInfo.InvariantCulture),
                group.Sum(order => order.Total).ToString("0.00", CultureInfo.InvariantCulture)
            })
            .ToListAsync(cancellationToken);

        return new ReportTable(
            ["Event", "TicketsSold", "Revenue"],
            rows);
    }

    private static FileExportDto ToCsv(ReportTable table, string baseName) {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', table.Headers.Select(EscapeCsv)));

        foreach (var row in table.Rows) {
            builder.AppendLine(string.Join(',', row.Select(EscapeCsv)));
        }

        return new FileExportDto {
            Content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray(),
            ContentType = "text/csv",
            FileName = $"{baseName}.csv"
        };
    }

    private static FileExportDto ToExcel(ReportTable table, string baseName) {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Report");

        for (var column = 0; column < table.Headers.Length; column++) {
            sheet.Cell(1, column + 1).Value = table.Headers[column];
            sheet.Cell(1, column + 1).Style.Font.Bold = true;
        }

        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++) {
            var row = table.Rows[rowIndex];
            for (var column = 0; column < row.Length; column++) {
                sheet.Cell(rowIndex + 2, column + 1).Value = row[column];
            }
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new FileExportDto {
            Content = stream.ToArray(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = $"{baseName}.xlsx"
        };
    }

    private static string EscapeCsv(string value) {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r')) {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private sealed record ReportTable(string[] Headers, List<string[]> Rows);
}
