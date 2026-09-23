using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace ECommerce.Tests.Integration;

public class ProductsApiTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductsApiTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProducts_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Products");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnOk_WhenCustomerIsAuthenticated()
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

        var response = await client.GetAsync("/api/Products");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnForbidden_WhenCustomerIsAuthenticated()
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
            name = "Test Product",
            description = "Integration Test Product",
            price = 100,
            stockQuantity = 10,
            sku = "TEST-001",
            categoryId = 1
        };

        var response = await client.PostAsJsonAsync(
            "/api/Products",
            request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }
    [Fact]
    public async Task CreateProduct_ShouldReturnCreated_WhenAdminIsAuthenticated()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
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

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "AdminTest");

        var request = new
        {
            name = "Integration Test Product",
            description = "Created by integration test",
            price = 100,
            stockQuantity = 10,
            sku = $"INT-TEST-{Guid.NewGuid():N}",
            categoryId = 1
        };

        var response = await client.PostAsJsonAsync(
            "/api/Products",
            request);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.IsSuccessStatusCode,
            $"Create product failed. Status: {response.StatusCode}, Body: {responseBody}");
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnBadRequest_WhenPriceIsInvalid()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
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

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "AdminTest");

        var request = new
        {
            name = "Invalid Product",
            description = "Invalid price test",
            price = 0,
            stockQuantity = 10,
            sku = "INVALID-PRICE-001",
            categoryId = 1
        };

        var response = await client.PostAsJsonAsync(
            "/api/Products",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenProductDoesNotExist()
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

        var response = await client.GetAsync(
            "/api/Products/999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnBadRequest_WhenSkuAlreadyExists()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
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

        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "AdminTest");

        var request = new
        {
            name = "Duplicate SKU Product",
            description = "Testing duplicate SKU",
            price = 200,
            stockQuantity = 10,
            sku = "WH-001",
            categoryId = 1
        };

        var response = await client.PostAsJsonAsync(
            "/api/Products",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}