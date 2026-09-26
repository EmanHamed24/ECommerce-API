using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using ECommerce.Tests.Integration.TestData;

namespace ECommerce.Tests.Integration;

public class CartApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CartApiTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCart_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Cart");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetCart_ShouldReturnOk_WhenCustomerIsAuthenticated()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<
                    Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                    TestAuthHandler>(
                    "Test",
                    options => { });
            });
        });

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Test");

        var response = await client.GetAsync("/api/Cart");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_ShouldReturnOk_WhenCustomerAddsValidProduct()
    {
        // Create an Admin client to create a valid product for this test
        var adminFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "AdminTest";
                    options.DefaultChallengeScheme = "AdminTest";
                })
                .AddScheme<
                    Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                    AdminTestAuthHandler>(
                    "AdminTest",
                    options => { });
            });
        });

        var adminClient = adminFactory.CreateClient();

        adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "AdminTest");

        var productRequest = new
        {
            name = "Cart Integration Test Product",
            description = "Product created for cart integration test",
            price = 100,
            stockQuantity = 10,
            sku = $"CART-TEST-{Guid.NewGuid():N}",
            categoryId = 1
        };

        var createProductResponse = await adminClient.PostAsJsonAsync(
            "/api/Products",
            productRequest);

        var createProductBody =
            await createProductResponse.Content.ReadAsStringAsync();

        Assert.True(
            createProductResponse.IsSuccessStatusCode,
            $"Product creation failed. Status: {createProductResponse.StatusCode}, Body: {createProductBody}");

        var createdProduct =
            await createProductResponse.Content
                .ReadFromJsonAsync<
                    ECommerce.Application.DTOs.Products.ProductResponse>();

        Assert.NotNull(createdProduct);

        Assert.True(
            createdProduct.StockQuantity > 0,
            $"Created product has invalid stock quantity: {createdProduct.StockQuantity}");

        Assert.Equal(
            productRequest.sku,
            createdProduct.SKU);

        // Create a Customer client to add the product to the cart
        var customerFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<
                    Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                    TestAuthHandler>(
                    "Test",
                    options => { });
            });
        });

        var customerClient = customerFactory.CreateClient();

        customerClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Test");

        var request = new
        {
            productId = createdProduct.Id,
            quantity = 1
        };

        var response = await customerClient.PostAsJsonAsync(
            "/api/Cart/items",
            request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Assert.True(
            response.IsSuccessStatusCode,
            $"Add to cart failed. Status: {response.StatusCode}, Body: {responseBody}");
    }

    [Fact]
    public async Task AddToCart_ShouldReturnBadRequest_WhenQuantityExceedsStock()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<
                    Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                    TestAuthHandler>(
                    "Test",
                    options => { });
            });
        });

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Test");

        var request = new
        {
            productId = 1,
            quantity = 999999
        };

        var response = await client.PostAsJsonAsync(
            "/api/Cart/items",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<
                    Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                    TestAuthHandler>(
                    "Test",
                    options => { });
            });
        });

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Test");

        var request = new
        {
            productId = 999999,
            quantity = 1
        };

        var response = await client.PostAsJsonAsync(
            "/api/Cart/items",
            request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}
