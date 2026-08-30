using Application.DTOs.Images;

namespace Application.Interfaces;

public interface IImageService {
    Task<ImageDto> SaveAsync(
        string storedFileName,
        string originalFileName,
        string url,
        string contentType,
        long sizeInBytes,
        Guid? uploadedByUserId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid imageId,
        CancellationToken cancellationToken = default);
}
