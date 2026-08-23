using Application.Interfaces;
using MediatR;

namespace Application.Features.Categories.Commands;

public sealed record DeleteCategoryCommand(Guid CategoryId) : IRequest<Unit>;

public sealed class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand, Unit> {
    private readonly ICategoryService _categoryService;

    public DeleteCategoryCommandHandler(ICategoryService categoryService) {
        _categoryService = categoryService;
    }

    public async Task<Unit> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken) {
        await _categoryService.DeleteAsync(
            request.CategoryId,
            cancellationToken);

        return Unit.Value;
    }
}
