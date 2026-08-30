using Application.Features.Users.Commands;
using FluentValidation;

namespace Application.Validators.Users;

public sealed class UploadAvatarCommandValidator
    : AbstractValidator<UploadAvatarCommand> {
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp"];

    private static readonly string[] AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".webp"];

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public UploadAvatarCommandValidator() {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .Must(HaveValidExtension).WithMessage("Only jpg, png, and webp files are allowed.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(type => AllowedContentTypes.Contains(type))
            .WithMessage("Only jpg, png, and webp files are allowed.");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File cannot be empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("File size cannot exceed 5 MB.");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File stream is required.");
    }

    private static bool HaveValidExtension(string fileName) {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}
