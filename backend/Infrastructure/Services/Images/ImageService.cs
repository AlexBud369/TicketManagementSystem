using Application.DTOs.Images;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Images;

public sealed class ImageService : IImageService {
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public ImageService(AppDbContext dbContext, IMapper mapper) {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ImageDto> SaveAsync(
        string storedFileName,
        string originalFileName,
        string url,
        string contentType,
        long sizeInBytes,
        Guid? uploadedByUserId,
        CancellationToken cancellationToken = default) {
        var image = new Image {
            FileName = Truncate(storedFileName, 260),
            OriginalFileName = Truncate(originalFileName, 260),
            Url = url,
            ContentType = contentType,
            SizeInBytes = sizeInBytes,
            UploadedByUserId = uploadedByUserId
        };

        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ImageDto>(image);
    }

    public async Task DeleteAsync(
        Guid imageId,
        CancellationToken cancellationToken = default) {
        var image = await _dbContext.Images
            .FirstOrDefaultAsync(item => item.Id == imageId, cancellationToken);

        if (image is null) {
            return;
        }

        var stillUsedAsAvatar = await _dbContext.AppUsers
            .AnyAsync(user => user.AvatarImageId == imageId, cancellationToken);

        var stillUsedAsBanner = await _dbContext.Events
            .AnyAsync(eventEntity => eventEntity.BannerImageId == imageId, cancellationToken);

        if (stillUsedAsAvatar || stillUsedAsBanner) {
            throw AppException.BusinessRule(
                "Image cannot be deleted while it is still assigned to a user or event.");
        }

        _dbContext.Images.Remove(image);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string Truncate(string value, int maxLength) {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength) {
            return value;
        }

        return value[..maxLength];
    }
}
