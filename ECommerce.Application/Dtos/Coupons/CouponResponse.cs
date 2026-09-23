namespace ECommerce.Application.DTOs.Coupons;

public class CouponResponse
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public decimal DiscountPercentage { get; set; }

    public DateTime ExpirationDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
