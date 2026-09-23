using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Application.Interfaces;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(int userId);

    Task<CartResponse> AddToCartAsync(
        int userId,
        AddToCartRequest request);

    Task RemoveFromCartAsync(
        int userId,
        int productId);

    Task ClearCartAsync(int userId);
}