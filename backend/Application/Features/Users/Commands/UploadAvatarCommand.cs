using Application.DTOs.Users;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Users.Commands;

public sealed record UploadAvatarCommand(
    Guid UserId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize
) : IRequest<UserDto>;

public sealed class UploadAvatarCommandHandler
    : IRequestHandler<UploadAvatarCommand, UserDto> {
    private readonly IUserService _userService;
    private readonly IStorageService _storageService;
    private readonly IImageService _imageService;

    public UploadAvatarCommandHandler(
        IUserService userService,
        IStorageService storageService,
        IImageService imageService) {
        _userService = userService;
        _storageService = storageService;
        _imageService = imageService;
    }

    public async Task<UserDto> Handle(
        UploadAvatarCommand request,
        CancellationToken cancellationToken) {
        var currentUser = await _userService.GetByIdAsync(
            request.UserId, cancellationToken);

        var uniqueFileName =
            $"avatars/{request.UserId}/{Guid.NewGuid()}{Path.GetExtension(request.FileName)}";

        var avatarUrl = await _storageService.UploadAsync(
            request.FileStream,
            uniqueFileName,
            request.ContentType,
            cancellationToken);

        var image = await _imageService.SaveAsync(
            uniqueFileName,
            request.FileName,
            avatarUrl,
            request.ContentType,
            request.FileSize,
            request.UserId,
            cancellationToken);

        await _userService.UpdateAvatarAsync(
            request.UserId, image.Id, cancellationToken);

        if (currentUser.AvatarImageId is Guid oldImageId) {
            if (!string.IsNullOrWhiteSpace(currentUser.AvatarUrl)) {
                await _storageService.DeleteAsync(
                    currentUser.AvatarUrl, cancellationToken);
            }

            await _imageService.DeleteAsync(oldImageId, cancellationToken);
        }

        return await _userService.GetByIdAsync(
            request.UserId, cancellationToken);
    }
}
