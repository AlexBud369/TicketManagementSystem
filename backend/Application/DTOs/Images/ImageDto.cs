namespace Application.DTOs.Images;

public sealed class ImageDto {
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
}
