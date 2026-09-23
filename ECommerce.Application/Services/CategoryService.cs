using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Mappings;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepository;

    public CategoryService(
        IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();

        return categories.Select(c => c.ToResponse());
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        return category?.ToResponse();
    }

    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request)
    {
        var category = request.ToEntity();

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return category.ToResponse();
    }
    public async Task UpdateAsync(
    int id,
    UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Category with ID {id} was not found.");
        }

        var existingCategory = await _categoryRepository
            .FirstOrDefaultAsync(c =>
                c.Name == request.Name &&
                c.Id != id);

        if (existingCategory is not null)
        {
            throw new BusinessException(
                $"Category with name '{request.Name}' already exists.");
        }

        request.ApplyToEntity(category);

        _categoryRepository.Update(category);

        await _categoryRepository.SaveChangesAsync();
    }
    public async Task DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Category with ID {id} was not found.");
        }

        _categoryRepository.Delete(category);

        await _categoryRepository.SaveChangesAsync();
    }
}
