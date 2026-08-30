using Application.Common;
using Application.DTOs.Events;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Events.Commands;

public sealed record UploadEventBannerCommand(
    Guid EventId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize
) : IRequest<EventDto>;

public sealed class UploadEventBannerCommandHandler
    : IRequestHandler<UploadEventBannerCommand, EventDto> {
    private readonly IEventService _eventService;
    private readonly IStorageService _storageService;
    private readonly IImageService _imageService;

    public UploadEventBannerCommandHandler(
        IEventService eventService,
        IStorageService storageService,
        IImageService imageService) {
        _eventService = eventService;
        _storageService = storageService;
        _imageService = imageService;
    }

    public async Task<EventDto> Handle(
        UploadEventBannerCommand request,
        CancellationToken cancellationToken) {
        var evt = await _eventService.GetByIdAsync(
            request.EventId, cancellationToken);

        Guard.AgainstNotFound(evt, "Event", request.EventId);

        var uniqueFileName =
            $"events/{request.EventId}/{Guid.NewGuid()}{Path.GetExtension(request.FileName)}";

        var bannerUrl = await _storageService.UploadAsync(
            request.FileStream,
            uniqueFileName,
            request.ContentType,
            cancellationToken);

        var image = await _imageService.SaveAsync(
            uniqueFileName,
            request.FileName,
            bannerUrl,
            request.ContentType,
            request.FileSize,
            null,
            cancellationToken);

        await _eventService.UpdateBannerAsync(
            request.EventId, image.Id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(evt.BannerImageUrl)) {
            await _storageService.DeleteAsync(
                evt.BannerImageUrl, cancellationToken);
        }

        return await _eventService.GetByIdAsync(
            request.EventId, cancellationToken);
    }
}
