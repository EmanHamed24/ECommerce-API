using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Tests.Integration.TestData;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.Integration;

public static class TestDatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // Category
        var category = await context.Categories
            .FirstOrDefaultAsync(
                c => c.Name == TestConstants.CategoryName);

        if (category is null)
        {
            category = new Category
            {
                Name = TestConstants.CategoryName,
                Description = "Category for integration tests",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }

        // Product
        var product = await context.Products
            .FirstOrDefaultAsync(
                p => p.SKU == TestConstants.ProductSku);

        if (product is null)
        {
            product = new Product
            {
                Name = "Test Wireless Headphones",
                Description = "Product for integration tests",
                Price = 1000m,
                StockQuantity = 20,
                SKU = TestConstants.ProductSku,
                Status = ProductStatuses.Active,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Products.Add(product);
        }
        else
        {
            product.StockQuantity = 20;
            product.Status = ProductStatuses.Active;
            product.Price = 1000m;
            product.CategoryId = category.Id;
        }

        await context.SaveChangesAsync();

        // Customer
        var customer = await context.Users
            .FirstOrDefaultAsync(
                u => u.Email == TestConstants.CustomerEmail);

        if (customer is null)
        {
            customer = new User
            {
                Name = "Test Customer",
                Email = TestConstants.CustomerEmail,
                PasswordHash = "TestPasswordHash",
                Role = Roles.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(customer);
            await context.SaveChangesAsync();
        }
    }
}