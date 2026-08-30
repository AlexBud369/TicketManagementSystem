using Application.Features.Categories.Commands;
using FluentValidation;

namespace Application.Validators.Categories;

public sealed class CreateCategoryCommandValidator
    : AbstractValidator<CreateCategoryCommand> {
    public CreateCategoryCommandValidator() {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Icon)
            .MaximumLength(500);
    }
}
