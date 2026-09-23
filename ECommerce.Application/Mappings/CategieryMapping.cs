using ECommerce.Application.DTOs.Categories;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public static class CategoryMapping
{
    public static Category ToEntity(
        this CreateCategoryRequest request)
    {
        return new Category
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true
        };
    }

    public static void ApplyToEntity(
        this UpdateCategoryRequest request, Category category)
    {
        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
    }

    public static CategoryResponse ToResponse(
        this Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }
}
