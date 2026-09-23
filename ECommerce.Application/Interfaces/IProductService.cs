using ECommerce.Application.DTOs.Products;

namespace ECommerce.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetAllAsync();

    Task<ProductResponse?> GetByIdAsync(int id);

    Task<ProductResponse> CreateAsync(CreateProductRequest request);

    Task UpdateAsync(int id, UpdateProductRequest request);

    Task DeleteAsync(int id);
}