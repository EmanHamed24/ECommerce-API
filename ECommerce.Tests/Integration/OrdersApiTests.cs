using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using ECommerce.Tests.Integration.TestData;

namespace ECommerce.Tests.Integration;

public class OrdersApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OrdersApiTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMyOrders_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Orders");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetMyOrders_ShouldReturnOk_WhenCustomerIsAuthenticated()
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

        var response = await client.GetAsync("/api/Orders");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnOk_WhenCustomerHasItemsInCart()
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

        var productsBody =
            await productsResponse.Content.ReadAsStringAsync();

        Assert.True(
            productsResponse.IsSuccessStatusCode,
            $"Products GET failed. Status: {productsResponse.StatusCode}, Body: {productsBody}");

        var products =
            System.Text.Json.JsonSerializer.Deserialize<
                List<ECommerce.Application.DTOs.Products.ProductResponse>>(
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

        var addToCartRequest = new
        {
            productId = testProduct!.Id,
            quantity = 1
        };

        var cartResponse = await client.PostAsJsonAsync(
            "/api/Cart/items",
            addToCartRequest);

        var cartResponseBody =
            await cartResponse.Content.ReadAsStringAsync();

        Assert.True(
            cartResponse.IsSuccessStatusCode,
            $"Add to cart failed. Status: {cartResponse.StatusCode}, Body: {cartResponseBody}");
        var orderResponse = await client.PostAsJsonAsync(
            "/api/Orders",
             new { });

        var orderResponseBody =
            await orderResponse.Content.ReadAsStringAsync();

        Assert.True(
            orderResponse.IsSuccessStatusCode,
            $"Create order failed. Status: {orderResponse.StatusCode}, Body: {orderResponseBody}");
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenCartIsEmpty()
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

        await client.DeleteAsync("/api/Cart");

        var response = await client.PostAsJsonAsync(
            "/api/Orders",
            new { });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}
