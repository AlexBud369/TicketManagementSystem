using Application.DTOs.Categories;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Categories.Queries;

public sealed record GetCategoryByIdQuery(Guid CategoryId)
    : IRequest<CategoryDto>;

public sealed class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto> {
    private readonly ICategoryService _categoryService;

    public GetCategoryByIdQueryHandler(ICategoryService categoryService) {
        _categoryService = categoryService;
    }

    public async Task<CategoryDto> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken) {
        return await _categoryService.GetByIdAsync(
            request.CategoryId,
            cancellationToken);
    }
}
