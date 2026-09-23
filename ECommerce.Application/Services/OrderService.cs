using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IRepository<Cart> _cartRepository;
    private readonly IRepository<CartItem> _cartItemRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Coupon> _couponRepository;

    public OrderService(
        IRepository<Order> orderRepository,
        IRepository<OrderItem> orderItemRepository,
        IRepository<Cart> cartRepository,
        IRepository<CartItem> cartItemRepository,
        IRepository<Product> productRepository,
        IRepository<Coupon> couponRepository)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
    }

    public async Task<OrderResponse> CreateOrderAsync(
        int userId,
        CreateOrderRequest request)
    {
        var cart = await _cartRepository
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            throw new BusinessException(
                "Cart is empty.");
        }

        var cartItems = (await _cartItemRepository.GetAllAsync(
            ci => ci.CartId == cart.Id))
            .ToList();

        if (cartItems.Count == 0)
        {
            throw new BusinessException(
                "Cart is empty.");
        }

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatuses.Pending,
            OrderDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        decimal totalAmount = 0;

        foreach (var cartItem in cartItems)
        {
            var product = await _productRepository
                .GetByIdAsync(cartItem.ProductId);

            if (product is null)
            {
                throw new BusinessException(
                    $"Product with ID {cartItem.ProductId} was not found.");
            }

            if (product.Status != ProductStatuses.Active)
            {
                throw new BusinessException(
                    $"Product '{product.Name}' is not available.");
            }

            if (cartItem.Quantity > product.StockQuantity)
            {
                throw new BusinessException(
                    $"Insufficient stock for product '{product.Name}'.");
            }

            var unitPrice = product.Price;

            var totalPrice =
                unitPrice * cartItem.Quantity;

            var orderItem = new OrderItem
            {
                Order = order,
                ProductId = product.Id,
                Quantity = cartItem.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice
            };

            order.OrderItems.Add(orderItem);

            totalAmount += totalPrice;

            product.StockQuantity -= cartItem.Quantity;

            if (product.StockQuantity == 0)
            {
                product.Status = ProductStatuses.OutOfStock;
            }

            _productRepository.Update(product);
        }

        decimal discountAmount = 0;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var coupon = await _couponRepository
                .FirstOrDefaultAsync(c =>
                    c.Code == request.CouponCode);

            if (coupon is null)
            {
                throw new BusinessException(
                    "Coupon was not found.");
            }

            if (!coupon.IsActive)
            {
                throw new BusinessException(
                    "Coupon is inactive.");
            }

            if (coupon.ExpirationDate < DateTime.UtcNow)
            {
                throw new BusinessException(
                    "Coupon has expired.");
            }

            discountAmount =
                totalAmount *
                (coupon.DiscountPercentage / 100m);

            order.CouponId = coupon.Id;
        }

        order.TotalAmount = totalAmount;
        order.DiscountAmount = discountAmount;
        order.FinalAmount = totalAmount - discountAmount;

        await _orderRepository.AddAsync(order);

        foreach (var cartItem in cartItems)
        {
            _cartItemRepository.Delete(cartItem);
        }

        await _orderRepository.SaveChangesAsync();
        await _cartItemRepository.SaveChangesAsync();

        return new OrderResponse
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status,
            Items = order.OrderItems
                .Select(item => new OrderItemResponse
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                })
                .ToList()
        };
    }

    public async Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(
        int userId)
    {
        var orders = await _orderRepository
            .GetAllAsync(o => o.UserId == userId, o => o.OrderItems);

        return orders.Select(order => new OrderResponse
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status,
            Items = order.OrderItems
                .Select(item => new OrderItemResponse
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                })
                .ToList()
        });
    }

    public async Task<OrderResponse?> GetByIdAsync(
        int userId,
        int orderId)
    {
        var order = await _orderRepository
             .FirstOrDefaultAsync(
             o => o.Id == orderId && o.UserId == userId, o => o.OrderItems);

        if (order is null)
        {
            return null;
        }

        return new OrderResponse
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status,
            Items = order.OrderItems
                .Select(item => new OrderItemResponse
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                })
                .ToList()
        };
    }
}
