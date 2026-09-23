using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using Moq;

namespace ECommerce.Tests.Unit;

public class CategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateCategorySuccessfully()
    {
        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        var service = new CategoryService(
            categoryRepositoryMock.Object);

        var request = new CreateCategoryRequest
        {
            Name = "Electronics",
            Description = "Electronic products"
        };

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Electronics", result.Name);
        Assert.Equal("Electronic products", result.Description);

        categoryRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Category>(c =>
                c.Name == "Electronics" &&
                c.Description == "Electronic products")),
            Times.Once);

        categoryRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
    {
        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Category
            {
                Id = 1,
                Name = "Electronics",
                Description = "Electronic products",
                IsActive = true
            });

        var service = new CategoryService(
            categoryRepositoryMock.Object);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Electronics", result.Name);
        Assert.Equal("Electronic products", result.Description);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var service = new CategoryService(
            categoryRepositoryMock.Object);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategorySuccessfully()
    {
        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        var category = new Category
        {
            Id = 1,
            Name = "Old Category",
            Description = "Old Description",
            IsActive = true
        };

        categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        categoryRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync((Category?)null);

        var service = new CategoryService(
            categoryRepositoryMock.Object);

        var request = new UpdateCategoryRequest
        {
            Name = "Updated Category",
            Description = "Updated Description",
            IsActive = false
        };

        await service.UpdateAsync(1, request);

        Assert.Equal("Updated Category", category.Name);
        Assert.Equal("Updated Description", category.Description);
        Assert.False(category.IsActive);

        categoryRepositoryMock.Verify(
            r => r.Update(category),
            Times.Once);

        categoryRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCategoryDoesNotExist()
    {
        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var service = new CategoryService(
            categoryRepositoryMock.Object);

        var request = new UpdateCategoryRequest
        {
            Name = "Updated Category",
            Description = "Updated Description",
            IsActive = true
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateAsync(999, request));

        Assert.Equal(
            "Category with ID 999 was not found.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCategoryNameAlreadyExists()
    {
        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Category
            {
                Id = 1,
                Name = "Old Category",
                Description = "Old Description",
                IsActive = true
            });

        categoryRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(new Category
            {
                Id = 2,
                Name = "Electronics",
                Description = "Another category",
                IsActive = true
            });

        var service = new CategoryService(
            categoryRepositoryMock.Object);

        var request = new UpdateCategoryRequest
        {
            Name = "Electronics",
            Description = "Updated Description",
            IsActive = true
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.UpdateAsync(1, request));

        Assert.Equal(
            "Category with name 'Electronics' already exists.",
            exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenCategoryDoesNotExist()
    {
        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var service = new CategoryService(
            categoryRepositoryMock.Object);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DeleteAsync(999));

        Assert.Equal(
            "Category with ID 999 was not found.",
            exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategorySuccessfully()
    {
        var categoryRepositoryMock =
            new Mock<IRepository<Category>>();

        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic products",
            IsActive = true
        };

        categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);

        var service = new CategoryService(
            categoryRepositoryMock.Object);

        await service.DeleteAsync(1);

        categoryRepositoryMock.Verify(
            r => r.Delete(category),
            Times.Once);

        categoryRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }
}
