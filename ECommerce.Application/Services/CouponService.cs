using ECommerce.Application.DTOs.Coupons;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Application.Services;

public class CouponService : ICouponService
{
    private readonly IRepository<Coupon> _couponRepository;

    public CouponService(
        IRepository<Coupon> couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<IEnumerable<CouponResponse>> GetAllAsync()
    {
        var coupons = await _couponRepository.GetAllAsync();

        return coupons.Select(ToResponse);
    }

    public async Task<CouponResponse?> GetByIdAsync(int id)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);

        return coupon is null
            ? null
            : ToResponse(coupon);
    }

    public async Task<CouponResponse> CreateAsync(
        CreateCouponRequest request)
    {
        if (request.DiscountPercentage <= 0 ||
            request.DiscountPercentage > 100)
        {
            throw new BusinessException(
                "Discount percentage must be greater than 0 and less than or equal to 100.");
        }

        if (request.ExpirationDate <= DateTime.UtcNow)
        {
            throw new BusinessException(
                "Expiration date must be in the future.");
        }

        var existingCoupon = await _couponRepository
            .FirstOrDefaultAsync(c =>
                c.Code == request.Code);

        if (existingCoupon is not null)
        {
            throw new BusinessException(
                $"Coupon with code '{request.Code}' already exists.");
        }

        var coupon = new Coupon
        {
            Code = request.Code,
            DiscountPercentage = request.DiscountPercentage,
            ExpirationDate = request.ExpirationDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _couponRepository.AddAsync(coupon);
        await _couponRepository.SaveChangesAsync();

        return ToResponse(coupon);
    }

    public async Task UpdateAsync(
        int id,
        UpdateCouponRequest request)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);

        if (coupon is null)
        {
            throw new KeyNotFoundException(
                $"Coupon with ID {id} was not found.");
        }

        if (request.DiscountPercentage <= 0 ||
            request.DiscountPercentage > 100)
        {
            throw new BusinessException(
                "Discount percentage must be greater than 0 and less than or equal to 100.");
        }

        if (request.ExpirationDate <= DateTime.UtcNow)
        {
            throw new BusinessException(
                "Expiration date must be in the future.");
        }

        coupon.DiscountPercentage =
            request.DiscountPercentage;

        coupon.ExpirationDate =
            request.ExpirationDate;

        coupon.IsActive =
            request.IsActive;

        _couponRepository.Update(coupon);

        await _couponRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);

        if (coupon is null)
        {
            throw new KeyNotFoundException(
                $"Coupon with ID {id} was not found.");
        }

        _couponRepository.Delete(coupon);

        await _couponRepository.SaveChangesAsync();
    }

    private static CouponResponse ToResponse(
        Coupon coupon)
    {
        return new CouponResponse
        {
            Id = coupon.Id,
            Code = coupon.Code,
            DiscountPercentage =
                coupon.DiscountPercentage,
            ExpirationDate =
                coupon.ExpirationDate,
            IsActive =
                coupon.IsActive,
            CreatedAt =
                coupon.CreatedAt
        };
    }
}
