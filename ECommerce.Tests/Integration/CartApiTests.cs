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

        var productsResponse = await client.GetAsync(
            "/api/Products");

        var productsBody = await productsResponse.Content
            .ReadAsStringAsync();

        Assert.True(
            productsResponse.IsSuccessStatusCode,
            $"Products GET failed. Status: {productsResponse.StatusCode}, Body: {productsBody}");

        var products = System.Text.Json.JsonSerializer
              .Deserialize<List<ECommerce.Application.DTOs.Products.ProductResponse>>(
              productsBody,
        new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var testProduct = products?
            .FirstOrDefault(p => p.SKU == TestConstants.ProductSku);

        if (testProduct == null)
        {
            throw new Exception(
                $"TestConstants.ProductSku was not found. Products response: {productsBody}");
        }

        var request = new
        {
            productId = testProduct.Id,
            quantity = 1
        };

        var productResponse = await client.GetAsync(
            $"/api/Products/{testProduct.Id}");

        var productBody = await productResponse.Content
            .ReadAsStringAsync();

        Assert.True(
            productResponse.IsSuccessStatusCode,
            $"Product GET failed. Status: {productResponse.StatusCode}, Body: {productBody}");

        Assert.True(
            productBody.Contains("active", StringComparison.OrdinalIgnoreCase),
            $"TEST PRODUCT DATA: {productBody}");
        Console.WriteLine(
            $"Test product response: {productResponse.StatusCode}, Body: {productBody}");

        var response = await client.PostAsJsonAsync(
            "/api/Cart/items",
            request);

        var responseBody = await response.Content.ReadAsStringAsync();

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
