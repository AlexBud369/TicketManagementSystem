namespace Application.DTOs.Reports;

public enum ReportType {
    Sales = 0,
    Users = 1,
    Orders = 2,
    Revenue = 3
}

public enum ReportFormat {
    Csv = 0,
    Excel = 1
}

public sealed class FileExportDto {
    public byte[] Content { get; init; } = [];
    public string ContentType { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}
