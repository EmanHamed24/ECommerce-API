using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using Moq;

namespace ECommerce.Tests.Unit;

public class ProductServiceTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;

    public ProductServiceTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenSkuAlreadyExists()
    {
        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        productRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Existing Product",
                SKU = "SKU-001"
            });

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        var request = new CreateProductRequest
        {
            Name = "New Product",
            Description = "Test Product",
            Price = 100,
            StockQuantity = 10,
            SKU = "SKU-001",
            CategoryId = 1
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(request));

        Assert.Equal(
            "Product with SKU 'SKU-001' already exists.",
            exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCategoryDoesNotExist()
    {
        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        productRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync((Product?)null);

        categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 100,
            StockQuantity = 10,
            SKU = "TEST-001",
            CategoryId = 999
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAsync(request));

        Assert.Equal(
            "Category with ID 999 was not found.",
            exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProductSuccessfully()
    {
        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        productRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync((Product?)null);

        categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Category
            {
                Id = 1,
                Name = "Electronics"
            });

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        var request = new CreateProductRequest
        {
            Name = "Wireless Mouse",
            Description = "Test wireless mouse",
            Price = 500,
            StockQuantity = 20,
            SKU = "TEST-MOUSE-001",
            CategoryId = 1
        };

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Wireless Mouse", result.Name);
        Assert.Equal(500, result.Price);
        Assert.Equal(20, result.StockQuantity);
        Assert.Equal("TEST-MOUSE-001", result.SKU);
        Assert.Equal(1, result.CategoryId);

        productRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Product>(p =>
                p.Name == "Wireless Mouse" &&
                p.Price == 500 &&
                p.StockQuantity == 20 &&
                p.SKU == "TEST-MOUSE-001" &&
                p.CategoryId == 1)),
            Times.Once);

        productRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);

        _cacheServiceMock.Verify(
            c => c.RemoveAsync("products:all"),
            Times.Once);
    }


    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Wireless Mouse",
                Description = "Test wireless mouse",
                Price = 500,
                StockQuantity = 20,
                SKU = "WM-001",
                Status = "Active",
                CategoryId = 1
            });

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Wireless Mouse", result.Name);
        Assert.Equal("WM-001", result.SKU);
        Assert.Equal(500, result.Price);
        Assert.Equal(20, result.StockQuantity);
        Assert.Equal("Active", result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProductSuccessfully()
    {
        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        var product = new Product
        {
            Id = 1,
            Name = "Old Product",
            Description = "Old Description",
            Price = 100,
            StockQuantity = 10,
            SKU = "OLD-001",
            Status = "Active",
            CategoryId = 1
        };

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        productRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync((Product?)null);

        categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new Category
            {
                Id = 2,
                Name = "Accessories"
            });

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 250,
            StockQuantity = 15,
            SKU = "NEW-001",
            CategoryId = 2
        };

        await service.UpdateAsync(1, request);

        Assert.Equal("Updated Product", product.Name);
        Assert.Equal("Updated Description", product.Description);
        Assert.Equal(250, product.Price);
        Assert.Equal(15, product.StockQuantity);
        Assert.Equal("NEW-001", product.SKU);
        Assert.Equal(2, product.CategoryId);

        productRepositoryMock.Verify(
            r => r.Update(product),
            Times.Once);

        productRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);

        _cacheServiceMock.Verify(
            c => c.RemoveAsync("products:all"),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenProductDoesNotExist()
    {
        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 250,
            StockQuantity = 15,
            SKU = "NEW-001",
            CategoryId = 1
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(999, request));

        Assert.Equal(
            "Product with ID 999 was not found.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenSkuAlreadyExists()
    {
        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Current Product",
                SKU = "CURRENT-001",
                CategoryId = 1
            });

        productRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(new Product
            {
                Id = 2,
                Name = "Another Product",
                SKU = "EXISTING-001",
                CategoryId = 1
            });

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 250,
            StockQuantity = 15,
            SKU = "EXISTING-001",
            CategoryId = 1
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.UpdateAsync(1, request));

        Assert.Equal(
            "Product with SKU 'EXISTING-001' already exists.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCategoryDoesNotExist()
    {
        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Current Product",
                SKU = "CURRENT-001",
                Price = 100,
                StockQuantity = 10,
                CategoryId = 1
            });

        productRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync((Product?)null);

        categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 250,
            StockQuantity = 15,
            SKU = "NEW-001",
            CategoryId = 999
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.UpdateAsync(1, request));

        Assert.Equal(
            "Category with ID 999 was not found.",
            exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenProductDoesNotExist()
    {
        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteAsync(999));

        Assert.Equal(
            "Product with ID 999 was not found.",
            exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProductSuccessfully()
    {
        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            SKU = "TEST-001",
            Price = 100,
            StockQuantity = 10,
            CategoryId = 1
        };

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        await service.DeleteAsync(1);

        productRepositoryMock.Verify(
            r => r.Delete(product),
            Times.Once);

        productRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);

        _cacheServiceMock.Verify(
            c => c.RemoveAsync("products:all"),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnCachedProducts_WhenCacheExists()
    {
        // Arrange
        var cachedProducts = new List<ProductResponse>
    {
        new ProductResponse
        {
            Id = 1,
            Name = "Cached Product",
            Description = "Cached product description",
            Price = 1500m,
            StockQuantity = 10,
            SKU = "CACHE-001",
            Status = "Active",
            CategoryId = 1
        }
    };

        var cachedJson = System.Text.Json.JsonSerializer.Serialize(cachedProducts);

        _cacheServiceMock
            .Setup(c => c.GetAsync("products:all"))
            .ReturnsAsync(cachedJson);

        var productRepositoryMock = new Mock<IRepository<Product>>();
        var categoryRepositoryMock = new Mock<IRepository<Category>>();

        var service = new ProductService(
            productRepositoryMock.Object,
            categoryRepositoryMock.Object,
            _cacheServiceMock.Object);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Cached Product", result.First().Name);

        productRepositoryMock.Verify(
            r => r.GetAllAsync(),
            Times.Never);

        _cacheServiceMock.Verify(
            c => c.GetAsync("products:all"),
            Times.Once);
    }
}