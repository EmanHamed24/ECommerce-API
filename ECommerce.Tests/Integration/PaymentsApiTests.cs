using ECommerce.Tests.Integration.TestData;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace ECommerce.Tests.Integration;

public class PaymentsApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PaymentsApiTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProcessPayment_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/Payments",
            new
            {
                orderId = 1
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task ProcessPayment_ShouldReturnOk_WhenCustomerPaysForOwnOrder()
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

        var cartResponse = await client.PostAsJsonAsync(
            "/api/Cart/items",
            new
            {
                productId = 1,
                quantity = 1
            });

        Assert.Equal(
            HttpStatusCode.OK,
            cartResponse.StatusCode);

        var orderResponse = await client.PostAsJsonAsync(
            "/api/Orders",
            new { });

        Assert.Equal(
            HttpStatusCode.OK,
            orderResponse.StatusCode);

        var order = await orderResponse.Content
            .ReadFromJsonAsync<
                System.Text.Json.JsonElement>();

        var orderId = order
            .GetProperty("id")
            .GetInt32();

        var paymentResponse = await client.PostAsJsonAsync(
            "/api/Payments",
            new
            {
                orderId = orderId
            });

        var paymentBody = await paymentResponse.Content.ReadAsStringAsync();

        Assert.True(
            paymentResponse.IsSuccessStatusCode,
            $"Payment failed. Status: {paymentResponse.StatusCode}, Body: {paymentBody}");
    }

    [Fact]
    public async Task ProcessPayment_ShouldReturnBadRequest_WhenOrderIsAlreadyPaid()
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

        Assert.Equal(
            HttpStatusCode.OK,
            productsResponse.StatusCode);

        var products = await productsResponse.Content
            .ReadFromJsonAsync<
                List<ECommerce.Application.DTOs.Products.ProductResponse>>();

        var testProduct = products?
            .FirstOrDefault(p =>
                p.SKU == TestConstants.ProductSku);

        Assert.NotNull(testProduct);

        var cartResponse = await client.PostAsJsonAsync(
            "/api/Cart/items",
            new
            {
                productId = testProduct!.Id,
                quantity = 1
            });

        Assert.Equal(
            HttpStatusCode.OK,
            cartResponse.StatusCode);

        var orderResponse = await client.PostAsJsonAsync(
            "/api/Orders",
            new { });

        Assert.Equal(
            HttpStatusCode.OK,
            orderResponse.StatusCode);

        var order = await orderResponse.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        var orderId = order
            .GetProperty("id")
            .GetInt32();

        var firstPaymentResponse = await client.PostAsJsonAsync(
            "/api/Payments",
            new
            {
                orderId = orderId
            });

        Assert.Equal(
            HttpStatusCode.OK,
            firstPaymentResponse.StatusCode);

        var secondPaymentResponse = await client.PostAsJsonAsync(
            "/api/Payments",
            new
            {
                orderId = orderId
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondPaymentResponse.StatusCode);
    }
}
