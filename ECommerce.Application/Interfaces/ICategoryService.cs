using ECommerce.Application.DTOs.Categories;

namespace ECommerce.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();

    Task<CategoryResponse?> GetByIdAsync(int id);

    Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request);

    Task DeleteAsync(int id);

    Task UpdateAsync(
    int id,
    UpdateCategoryRequest request);
}
