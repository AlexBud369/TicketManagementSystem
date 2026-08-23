using Application.Features.Events.Commands;
using FluentValidation;

namespace Application.Validators.Events;

public sealed class UploadEventBannerCommandValidator
    : AbstractValidator<UploadEventBannerCommand> {
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp"];

    private static readonly string[] AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".webp"];

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public UploadEventBannerCommandValidator() {
        RuleFor(x => x.EventId)
            .NotEmpty();

        RuleFor(x => x.FileName)
            .NotEmpty()
            .Must(HaveValidExtension)
            .WithMessage("Only jpg, png, and webp files are allowed.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(type => AllowedContentTypes.Contains(type))
            .WithMessage("Only jpg, png, and webp files are allowed.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("File size cannot exceed 5 MB.");

        RuleFor(x => x.FileStream)
            .NotNull();
    }

    private static bool HaveValidExtension(string fileName) {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}
