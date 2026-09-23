namespace ECommerce.Application.DTOs.Coupons;

public class CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;

    public decimal DiscountPercentage { get; set; }

    public DateTime ExpirationDate { get; set; }
}
