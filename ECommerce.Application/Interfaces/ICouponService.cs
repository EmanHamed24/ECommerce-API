using ECommerce.Application.DTOs.Coupons;

namespace ECommerce.Application.Interfaces;

public interface ICouponService
{
    Task<IEnumerable<CouponResponse>> GetAllAsync();

    Task<CouponResponse?> GetByIdAsync(int id);

    Task<CouponResponse> CreateAsync(
        CreateCouponRequest request);

    Task UpdateAsync(
        int id,
        UpdateCouponRequest request);

    Task DeleteAsync(int id);
}