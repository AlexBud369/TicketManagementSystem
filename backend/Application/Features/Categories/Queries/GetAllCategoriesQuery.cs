using Application.DTOs.Categories;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Categories.Queries;

public sealed record GetAllCategoriesQuery
    : IRequest<IReadOnlyCollection<CategoryDto>>;

public sealed class GetAllCategoriesQueryHandler
    : IRequestHandler<GetAllCategoriesQuery, IReadOnlyCollection<CategoryDto>> {
    private readonly ICategoryService _categoryService;

    public GetAllCategoriesQueryHandler(ICategoryService categoryService) {
        _categoryService = categoryService;
    }

    public async Task<IReadOnlyCollection<CategoryDto>> Handle(
        GetAllCategoriesQuery request,
        CancellationToken cancellationToken) {
        return await _categoryService.GetAllAsync(cancellationToken);
    }
}
