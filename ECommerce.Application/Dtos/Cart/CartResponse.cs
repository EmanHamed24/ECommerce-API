namespace ECommerce.Application.DTOs.Cart;

public class CartResponse
{
    public int CartId { get; set; }

    public List<CartItemResponse> Items { get; set; } = new();

    public decimal TotalAmount { get; set; }
}
