namespace ECommerce.Application.Constants;

public static class CacheKeys
{
    public const string AllProducts = "products:all";

    public static string ProductById(int id)
        => $"products:{id}";
}
