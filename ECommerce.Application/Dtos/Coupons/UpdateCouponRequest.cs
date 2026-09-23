namespace ECommerce.Application.DTOs.Coupons;

public class UpdateCouponRequest
{
    public decimal DiscountPercentage { get; set; }

    public DateTime ExpirationDate { get; set; }

    public bool IsActive { get; set; }
}