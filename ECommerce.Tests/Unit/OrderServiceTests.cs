using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using Moq;

namespace ECommerce.Tests.Unit;

public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenCartIsEmpty()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var orderItemRepositoryMock =
            new Mock<IRepository<OrderItem>>();

        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var couponRepositoryMock =
            new Mock<IRepository<Coupon>>();

        cartRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>()))
            .ReturnsAsync(new Cart
            {
                Id = 1,
                UserId = 1
            });

        cartItemRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(new List<CartItem>());

        var service = new OrderService(
            orderRepositoryMock.Object,
            orderItemRepositoryMock.Object,
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object,
            couponRepositoryMock.Object);

        var request = new CreateOrderRequest();

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateOrderAsync(1, request));

        Assert.Equal(
            "Cart is empty.",
            exception.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenProductIsOutOfStock()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var orderItemRepositoryMock =
            new Mock<IRepository<OrderItem>>();

        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var couponRepositoryMock =
            new Mock<IRepository<Coupon>>();

        cartRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>()))
            .ReturnsAsync(new Cart
            {
                Id = 1,
                UserId = 1
            });

        var cartItems = new List<CartItem>
    {
        new CartItem
        {
            Id = 1,
            CartId = 1,
            ProductId = 1,
            Quantity = 1
        }
    };

        cartItemRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(cartItems);

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 0,
                Status = "OutOfStock"
            });

        var service = new OrderService(
            orderRepositoryMock.Object,
            orderItemRepositoryMock.Object,
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object,
            couponRepositoryMock.Object);

        var request = new CreateOrderRequest();

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateOrderAsync(1, request));

        Assert.Equal(
            "Product 'Test Product' is not available.",
            exception.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenStockIsInsufficient()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var orderItemRepositoryMock =
            new Mock<IRepository<OrderItem>>();

        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var couponRepositoryMock =
            new Mock<IRepository<Coupon>>();

        cartRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>()))
            .ReturnsAsync(new Cart
            {
                Id = 1,
                UserId = 1
            });

        var cartItems = new List<CartItem>
    {
        new CartItem
        {
            Id = 1,
            CartId = 1,
            ProductId = 1,
            Quantity = 5
        }
    };

        cartItemRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(cartItems);

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 2,
                Status = "Active"
            });

        var service = new OrderService(
            orderRepositoryMock.Object,
            orderItemRepositoryMock.Object,
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object,
            couponRepositoryMock.Object);

        var request = new CreateOrderRequest();

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateOrderAsync(1, request));

        Assert.Equal(
            "Insufficient stock for product 'Test Product'.",
            exception.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCreateOrderSuccessfully()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var orderItemRepositoryMock =
            new Mock<IRepository<OrderItem>>();

        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var couponRepositoryMock =
            new Mock<IRepository<Coupon>>();

        cartRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>()))
            .ReturnsAsync(new Cart
            {
                Id = 1,
                UserId = 1
            });

        var cartItems = new List<CartItem>
    {
        new CartItem
        {
            Id = 1,
            CartId = 1,
            ProductId = 1,
            Quantity = 2
        }
    };

        cartItemRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(cartItems);

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 10,
                Status = "Active"
            });

        var service = new OrderService(
            orderRepositoryMock.Object,
            orderItemRepositoryMock.Object,
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object,
            couponRepositoryMock.Object);

        var request = new CreateOrderRequest();

        var result = await service.CreateOrderAsync(1, request);

        Assert.NotNull(result);
        Assert.Equal(200, result.TotalAmount);
        Assert.Equal(0, result.DiscountAmount);
        Assert.Equal(200, result.FinalAmount);

        orderRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Order>(
                o =>
                    o.UserId == 1 &&
                    o.TotalAmount == 200 &&
                    o.FinalAmount == 200)),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldApplyCouponDiscount()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var orderItemRepositoryMock =
            new Mock<IRepository<OrderItem>>();

        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var couponRepositoryMock =
            new Mock<IRepository<Coupon>>();

        cartRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>()))
            .ReturnsAsync(new Cart
            {
                Id = 1,
                UserId = 1
            });

        var cartItems = new List<CartItem>
    {
        new CartItem
        {
            Id = 1,
            CartId = 1,
            ProductId = 1,
            Quantity = 2
        }
    };

        cartItemRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(cartItems);

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 10,
                Status = "Active"
            });

        couponRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>()))
            .ReturnsAsync(new Coupon
            {
                Id = 1,
                Code = "SAVE10",
                DiscountPercentage = 10,
                ExpirationDate = DateTime.UtcNow.AddDays(10),
                IsActive = true
            });

        var service = new OrderService(
            orderRepositoryMock.Object,
            orderItemRepositoryMock.Object,
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object,
            couponRepositoryMock.Object);

        var request = new CreateOrderRequest
        {
            CouponCode = "SAVE10"
        };

        var result = await service.CreateOrderAsync(1, request);

        Assert.NotNull(result);

        Assert.Equal(200, result.TotalAmount);
        Assert.Equal(20, result.DiscountAmount);
        Assert.Equal(180, result.FinalAmount);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenCouponDoesNotExist()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var orderItemRepositoryMock =
            new Mock<IRepository<OrderItem>>();

        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var couponRepositoryMock =
            new Mock<IRepository<Coupon>>();

        cartRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>()))
            .ReturnsAsync(new Cart
            {
                Id = 1,
                UserId = 1
            });

        var cartItems = new List<CartItem>
    {
        new CartItem
        {
            Id = 1,
            CartId = 1,
            ProductId = 1,
            Quantity = 2
        }
    };

        cartItemRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(cartItems);

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 10,
                Status = "Active"
            });

        couponRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>()))
            .ReturnsAsync((Coupon?)null);

        var service = new OrderService(
            orderRepositoryMock.Object,
            orderItemRepositoryMock.Object,
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object,
            couponRepositoryMock.Object);

        var request = new CreateOrderRequest
        {
            CouponCode = "INVALID"
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateOrderAsync(1, request));

        Assert.Equal(
            "Coupon was not found.",
            exception.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenCouponIsInactive()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var orderItemRepositoryMock =
            new Mock<IRepository<OrderItem>>();

        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var couponRepositoryMock =
            new Mock<IRepository<Coupon>>();

        cartRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>()))
            .ReturnsAsync(new Cart
            {
                Id = 1,
                UserId = 1
            });

        var cartItems = new List<CartItem>
    {
        new CartItem
        {
            Id = 1,
            CartId = 1,
            ProductId = 1,
            Quantity = 1
        }
    };

        cartItemRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(cartItems);

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 10,
                Status = "Active"
            });

        couponRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>()))
            .ReturnsAsync(new Coupon
            {
                Id = 1,
                Code = "SAVE10",
                DiscountPercentage = 10,
                ExpirationDate = DateTime.UtcNow.AddDays(10),
                IsActive = false
            });

        var service = new OrderService(
            orderRepositoryMock.Object,
            orderItemRepositoryMock.Object,
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object,
            couponRepositoryMock.Object);

        var request = new CreateOrderRequest
        {
            CouponCode = "SAVE10"
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateOrderAsync(1, request));

        Assert.Equal(
            "Coupon is inactive.",
            exception.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrow_WhenCouponIsExpired()
    {
        var orderRepositoryMock =
            new Mock<IRepository<Order>>();

        var orderItemRepositoryMock =
            new Mock<IRepository<OrderItem>>();

        var cartRepositoryMock =
            new Mock<IRepository<Cart>>();

        var cartItemRepositoryMock =
            new Mock<IRepository<CartItem>>();

        var productRepositoryMock =
            new Mock<IRepository<Product>>();

        var couponRepositoryMock =
            new Mock<IRepository<Coupon>>();

        cartRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Cart, bool>>>()))
            .ReturnsAsync(new Cart
            {
                Id = 1,
                UserId = 1
            });

        var cartItems = new List<CartItem>
    {
        new CartItem
        {
            Id = 1,
            CartId = 1,
            ProductId = 1,
            Quantity = 1
        }
    };

        cartItemRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(cartItems);

        productRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                StockQuantity = 10,
                Status = "Active"
            });

        couponRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>()))
            .ReturnsAsync(new Coupon
            {
                Id = 1,
                Code = "SAVE10",
                DiscountPercentage = 10,
                ExpirationDate = DateTime.UtcNow.AddDays(-1),
                IsActive = true
            });

        var service = new OrderService(
            orderRepositoryMock.Object,
            orderItemRepositoryMock.Object,
            cartRepositoryMock.Object,
            cartItemRepositoryMock.Object,
            productRepositoryMock.Object,
            couponRepositoryMock.Object);

        var request = new CreateOrderRequest
        {
            CouponCode = "SAVE10"
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateOrderAsync(1, request));

        Assert.Equal(
            "Coupon has expired.",
            exception.Message);
    }
}
