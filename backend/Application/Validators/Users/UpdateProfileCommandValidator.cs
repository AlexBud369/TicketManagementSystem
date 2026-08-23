using Application.Features.Users.Commands;
using FluentValidation;

namespace Application.Validators.Users;

public sealed class UpdateProfileCommandValidator
    : AbstractValidator<UpdateProfileCommand> {
    public UpdateProfileCommandValidator() {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.")
            .Matches(@"^[\d\+\-\(\)\s]*$").WithMessage("Phone format is invalid.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");
    }
}
