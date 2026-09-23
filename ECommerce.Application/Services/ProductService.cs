using System.Text.Json;
using ECommerce.Application.Constants;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Mappings;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly ICacheService _cacheService;

    public ProductService(
        IRepository<Product> productRepository,
        IRepository<Category> categoryRepository,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
    {
        var cachedData =
            await _cacheService.GetAsync(CacheKeys.AllProducts);

        if (cachedData is not null)
        {
            return JsonSerializer.Deserialize<List<ProductResponse>>(
                       cachedData)
                   ?? new List<ProductResponse>();
        }

        var products = await _productRepository.GetAllAsync();

        var result = products
            .Select(p => p.ToResponse())
            .ToList();

        var serializedData = JsonSerializer.Serialize(result);

        await _cacheService.SetAsync(
            CacheKeys.AllProducts,
            serializedData,
            TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var cacheKey = CacheKeys.ProductById(id);

        var cachedData =
            await _cacheService.GetAsync(cacheKey);

        if (cachedData is not null)
        {
            return JsonSerializer.Deserialize<ProductResponse>(
                cachedData);
        }

        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return null;
        }

        var result = product.ToResponse();

        var serializedData =
            JsonSerializer.Serialize(result);

        await _cacheService.SetAsync(
            cacheKey,
            serializedData,
            TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request)
    {
        var existingProduct = await _productRepository
            .FirstOrDefaultAsync(p => p.SKU == request.SKU);

        if (existingProduct is not null)
        {
            throw new BusinessException(
                $"Product with SKU '{request.SKU}' already exists.");
        }

        var category = await _categoryRepository
            .GetByIdAsync(request.CategoryId);

        if (category is null)
        {
            throw new BusinessException(
                $"Category with ID {request.CategoryId} was not found.");
        }

        var product = request.ToEntity();

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        await _cacheService.RemoveAsync(CacheKeys.AllProducts);

        return product.ToResponse();
    }

    public async Task UpdateAsync(
        int id,
        UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            throw new KeyNotFoundException(
                $"Product with ID {id} was not found.");
        }

        var existingProduct = await _productRepository
            .FirstOrDefaultAsync(
                p => p.SKU == request.SKU && p.Id != id);

        if (existingProduct is not null)
        {
            throw new BusinessException(
                $"Product with SKU '{request.SKU}' already exists.");
        }

        var category = await _categoryRepository
            .GetByIdAsync(request.CategoryId);

        if (category is null)
        {
            throw new BusinessException(
                $"Category with ID {request.CategoryId} was not found.");
        }

        request.ApplyToEntity(product);

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();

        await _cacheService.RemoveAsync(CacheKeys.AllProducts);

        await _cacheService.RemoveAsync(
            CacheKeys.ProductById(id));
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            throw new KeyNotFoundException(
                $"Product with ID {id} was not found.");
        }

        _productRepository.Delete(product);
        await _productRepository.SaveChangesAsync();

        await _cacheService.RemoveAsync(CacheKeys.AllProducts);

        await _cacheService.RemoveAsync(
            CacheKeys.ProductById(id));
    }
}