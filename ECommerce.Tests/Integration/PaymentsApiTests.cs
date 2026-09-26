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
            name = "Payment Integration Test Product",
            description = "Product created for payment integration test",
            price = 100,
            stockQuantity = 10,
            sku = $"PAYMENT-TEST-{Guid.NewGuid():N}",
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

        // Create a Customer client
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

        var client = customerFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Test");

        // Add the product to the customer's cart
        var cartResponse = await client.PostAsJsonAsync(
            "/api/Cart/items",
            new
            {
                productId = createdProduct.Id,
                quantity = 1
            });

        var cartBody =
            await cartResponse.Content.ReadAsStringAsync();

        Assert.True(
            cartResponse.IsSuccessStatusCode,
            $"Add to cart failed. Status: {cartResponse.StatusCode}, Body: {cartBody}");

        // Create the order
        var orderResponse = await client.PostAsJsonAsync(
            "/api/Orders",
            new { });

        var orderBody =
            await orderResponse.Content.ReadAsStringAsync();

        Assert.True(
            orderResponse.IsSuccessStatusCode,
            $"Create order failed. Status: {orderResponse.StatusCode}, Body: {orderBody}");

        var order =
            await orderResponse.Content
                .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        var orderId = order
            .GetProperty("id")
            .GetInt32();

        // First payment should succeed
        var firstPaymentResponse = await client.PostAsJsonAsync(
            "/api/Payments",
            new
            {
                orderId = orderId
            });

        var firstPaymentBody =
            await firstPaymentResponse.Content.ReadAsStringAsync();

        Assert.True(
            firstPaymentResponse.IsSuccessStatusCode,
            $"First payment failed. Status: {firstPaymentResponse.StatusCode}, Body: {firstPaymentBody}");

        // Second payment should be rejected because the order is already paid
        var secondPaymentResponse = await client.PostAsJsonAsync(
            "/api/Payments",
            new
            {
                orderId = orderId
            });

        var secondPaymentBody =
            await secondPaymentResponse.Content.ReadAsStringAsync();

        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondPaymentResponse.StatusCode);
    }
}
