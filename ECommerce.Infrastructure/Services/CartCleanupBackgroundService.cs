using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class CartCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CartCleanupBackgroundService> _logger;

    public CartCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<CartCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var context =
                    scope.ServiceProvider
                        .GetRequiredService<AppDbContext>();

                var cutoffDate =
                    DateTime.UtcNow.AddDays(-30);

                var oldCarts = await context.Carts
                    .Where(c =>
                        c.UpdatedAt < cutoffDate ||
                        (c.UpdatedAt == null &&
                         c.CreatedAt < cutoffDate))
                    .ToListAsync(stoppingToken);

                foreach (var cart in oldCarts)
                {
                    var cartItems = await context.CartItems
                        .Where(ci => ci.CartId == cart.Id)
                        .ToListAsync(stoppingToken);

                    context.CartItems.RemoveRange(cartItems);
                }

                if (oldCarts.Count > 0)
                {
                    await context.SaveChangesAsync(
                        stoppingToken);

                    _logger.LogInformation(
                        "Cleaned up {Count} old carts.",
                        oldCarts.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while cleaning old carts.");
            }

            await Task.Delay(
                TimeSpan.FromHours(24),
                stoppingToken);
        }
    }
}
