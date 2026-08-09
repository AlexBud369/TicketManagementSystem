using Domain.Common;

namespace Domain.Entities;

public class Image : BaseEntity {
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public Guid? UploadedByUserId { get; set; }
}
