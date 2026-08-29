using Application.DTOs.Categories;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Categories.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Categories;

public sealed class CategoryService : ICategoryService {
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public CategoryService(AppDbContext dbContext, IMapper mapper) {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default) {
        var category = new Category {
            Name = request.Name.Trim(),
            Description = NormalizeOptional(request.Description),
            Icon = NormalizeOptional(request.Icon)
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await MapAsync(category.Id, cancellationToken);
    }

    public async Task<CategoryDto> UpdateAsync(
        Guid categoryId,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default) {
        var category = await RequireAsync(categoryId, cancellationToken);

        category.Name = request.Name.Trim();
        category.Description = NormalizeOptional(request.Description);
        category.Icon = NormalizeOptional(request.Icon);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await MapAsync(category.Id, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default) {
        var category = await RequireAsync(categoryId, cancellationToken);

        var hasEvents = await _dbContext.Events
            .AnyAsync(eventEntity => eventEntity.CategoryId == categoryId, cancellationToken);

        if (hasEvents) {
            throw AppException.BusinessRule(
                "Category cannot be deleted while it still has events.");
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CategoryDto> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default) {
        return await MapAsync(categoryId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default) {
        var categories = await _mapper
            .ProjectTo<CategoryDto>(
                _dbContext.Categories.AsNoTracking().OrderBy(category => category.Name))
            .ToListAsync(cancellationToken);

        return categories;
    }

    public async Task<bool> ExistsAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default) {
        return await _dbContext.Categories
            .AnyAsync(category => category.Id == categoryId, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        Guid? excludeCategoryId = null,
        CancellationToken cancellationToken = default) {
        var normalized = name.Trim().ToLower();

        return await _dbContext.Categories
            .AnyAsync(
                category =>
                    category.Name.ToLower() == normalized &&
                    (!excludeCategoryId.HasValue || category.Id != excludeCategoryId.Value),
                cancellationToken);
    }

    private async Task<Category> RequireAsync(
        Guid categoryId,
        CancellationToken cancellationToken) {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(item => item.Id == categoryId, cancellationToken);

        if (category is null) {
            throw AppException.NotFound("Category", categoryId);
        }

        return category;
    }

    private async Task<CategoryDto> MapAsync(
        Guid categoryId,
        CancellationToken cancellationToken) {
        var dto = await _mapper
            .ProjectTo<CategoryDto>(
                _dbContext.Categories.AsNoTracking().Where(category => category.Id == categoryId))
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null) {
            throw AppException.NotFound("Category", categoryId);
        }

        return dto;
    }

    private static string? NormalizeOptional(string? value) {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
