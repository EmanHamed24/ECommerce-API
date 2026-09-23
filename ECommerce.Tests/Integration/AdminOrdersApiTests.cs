using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Tests.Integration;

public class AdminOrdersApiTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminOrdersApiTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturnOk_WhenAdminIsAuthenticated()
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

        var response = await client.GetAsync(
            "/api/admin/orders");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }
}
