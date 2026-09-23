using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using Moq;

namespace ECommerce.Tests.Unit;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldCreateCustomerSuccessfully()
    {
        var userRepositoryMock =
            new Mock<IRepository<User>>();

        var jwtTokenServiceMock =
            new Mock<IJwtTokenService>();

        userRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        var service = new AuthService(
            userRepositoryMock.Object,
            jwtTokenServiceMock.Object);

        var request = new RegisterRequest
        {
            Name = "Test Customer",
            Email = "customer@test.com",
            Password = "Password123!"
        };

        var result = await service.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Test Customer", result.Name);
        Assert.Equal("customer@test.com", result.Email);
        Assert.Equal(Roles.Customer, result.Role);

        userRepositoryMock.Verify(
            r => r.AddAsync(It.Is<User>(u =>
                u.Name == "Test Customer" &&
                u.Email == "customer@test.com" &&
                u.Role == Roles.Customer &&
                u.IsActive)),
            Times.Once);

        userRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);

        jwtTokenServiceMock.Verify(
            j => j.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var userRepositoryMock =
            new Mock<IRepository<User>>();

        var jwtTokenServiceMock =
            new Mock<IJwtTokenService>();

        var existingUser = new User
        {
            Id = 1,
            Name = "Existing User",
            Email = "existing@test.com",
            PasswordHash = "hashed-password",
            Role = Roles.Customer,
            IsActive = true
        };

        userRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(existingUser);

        var service = new AuthService(
            userRepositoryMock.Object,
            jwtTokenServiceMock.Object);

        var request = new RegisterRequest
        {
            Name = "New User",
            Email = "existing@test.com",
            Password = "Password123!"
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.RegisterAsync(request));

        Assert.Equal(
            "A user with this email already exists.",
            exception.Message);

        userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>()),
            Times.Never);

        userRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAdminAsync_ShouldCreateAdminSuccessfully()
    {
        var userRepositoryMock =
            new Mock<IRepository<User>>();

        var jwtTokenServiceMock =
            new Mock<IJwtTokenService>();

        userRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        jwtTokenServiceMock
            .Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("admin-test-token");

        var service = new AuthService(
            userRepositoryMock.Object,
            jwtTokenServiceMock.Object);

        var request = new CreateAdminRequest
        {
            Name = "Test Admin",
            Email = "admin@test.com",
            Password = "AdminPassword123!"
        };

        var result = await service.CreateAdminAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Test Admin", result.Name);
        Assert.Equal("admin@test.com", result.Email);
        Assert.Equal(Roles.Admin, result.Role);
        Assert.Equal("admin-test-token", result.Token);

        userRepositoryMock.Verify(
            r => r.AddAsync(It.Is<User>(u =>
                u.Name == "Test Admin" &&
                u.Email == "admin@test.com" &&
                u.Role == Roles.Admin &&
                u.IsActive)),
            Times.Once);

        userRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);

        jwtTokenServiceMock.Verify(
            j => j.GenerateToken(It.Is<User>(u =>
                u.Role == Roles.Admin)),
            Times.Once);
    }

    [Fact]
    public async Task CreateAdminAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var userRepositoryMock =
            new Mock<IRepository<User>>();

        var jwtTokenServiceMock =
            new Mock<IJwtTokenService>();

        var existingUser = new User
        {
            Id = 1,
            Name = "Existing User",
            Email = "admin@test.com",
            PasswordHash = "hashed-password",
            Role = Roles.Customer,
            IsActive = true
        };

        userRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(existingUser);

        var service = new AuthService(
            userRepositoryMock.Object,
            jwtTokenServiceMock.Object);

        var request = new CreateAdminRequest
        {
            Name = "New Admin",
            Email = "admin@test.com",
            Password = "AdminPassword123!"
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.CreateAdminAsync(request));

        Assert.Equal(
            "A user with this email already exists.",
            exception.Message);

        userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>()),
            Times.Never);

        userRepositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Never);

        jwtTokenServiceMock.Verify(
            j => j.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldLoginSuccessfully()
    {
        var userRepositoryMock =
            new Mock<IRepository<User>>();

        var jwtTokenServiceMock =
            new Mock<IJwtTokenService>();

        var password = "Password123!";

        var user = new User
        {
            Id = 1,
            Name = "Test Customer",
            Email = "customer@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = Roles.Customer,
            IsActive = true
        };

        userRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        jwtTokenServiceMock
            .Setup(j => j.GenerateToken(user))
            .Returns("customer-test-token");

        var service = new AuthService(
            userRepositoryMock.Object,
            jwtTokenServiceMock.Object);

        var request = new LoginRequest
        {
            Email = "customer@test.com",
            Password = password
        };

        var result = await service.LoginAsync(request);

        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal("Test Customer", result.Name);
        Assert.Equal("customer@test.com", result.Email);
        Assert.Equal(Roles.Customer, result.Role);
        Assert.Equal("customer-test-token", result.Token);

        jwtTokenServiceMock.Verify(
            j => j.GenerateToken(user),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var userRepositoryMock =
            new Mock<IRepository<User>>();

        var jwtTokenServiceMock =
            new Mock<IJwtTokenService>();

        userRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        var service = new AuthService(
            userRepositoryMock.Object,
            jwtTokenServiceMock.Object);

        var request = new LoginRequest
        {
            Email = "notfound@test.com",
            Password = "Password123!"
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.LoginAsync(request));

        Assert.Equal(
            "Invalid email or password.",
            exception.Message);

        jwtTokenServiceMock.Verify(
            j => j.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsIncorrect()
    {
        var userRepositoryMock =
            new Mock<IRepository<User>>();

        var jwtTokenServiceMock =
            new Mock<IJwtTokenService>();

        var user = new User
        {
            Id = 1,
            Name = "Test Customer",
            Email = "customer@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                "CorrectPassword123!"),
            Role = Roles.Customer,
            IsActive = true
        };

        userRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        var service = new AuthService(
            userRepositoryMock.Object,
            jwtTokenServiceMock.Object);

        var request = new LoginRequest
        {
            Email = "customer@test.com",
            Password = "WrongPassword123!"
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.LoginAsync(request));

        Assert.Equal(
            "Invalid email or password.",
            exception.Message);

        jwtTokenServiceMock.Verify(
            j => j.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserIsInactive()
    {
        var userRepositoryMock =
            new Mock<IRepository<User>>();

        var jwtTokenServiceMock =
            new Mock<IJwtTokenService>();

        var user = new User
        {
            Id = 1,
            Name = "Inactive User",
            Email = "inactive@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                "Password123!"),
            Role = Roles.Customer,
            IsActive = false
        };

        userRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        var service = new AuthService(
            userRepositoryMock.Object,
            jwtTokenServiceMock.Object);

        var request = new LoginRequest
        {
            Email = "inactive@test.com",
            Password = "Password123!"
        };

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.LoginAsync(request));

        Assert.Equal(
            "This account is inactive.",
            exception.Message);

        jwtTokenServiceMock.Verify(
            j => j.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAdminAsync_ShouldHashPassword()
    {
        var userRepositoryMock =
            new Mock<IRepository<User>>();

        var jwtTokenServiceMock =
            new Mock<IJwtTokenService>();

        User? createdAdmin = null;

        userRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => createdAdmin = user);

        jwtTokenServiceMock
            .Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("admin-test-token");

        var service = new AuthService(
            userRepositoryMock.Object,
            jwtTokenServiceMock.Object);

        var request = new CreateAdminRequest
        {
            Name = "Test Admin",
            Email = "admin2@test.com",
            Password = "AdminPassword123!"
        };

        await service.CreateAdminAsync(request);

        Assert.NotNull(createdAdmin);
        Assert.NotEqual(
            request.Password,
            createdAdmin.PasswordHash);

        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                createdAdmin.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        var userRepositoryMock =
            new Mock<IRepository<User>>();

        var jwtTokenServiceMock =
            new Mock<IJwtTokenService>();

        User? createdUser = null;

        userRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => createdUser = user);

        var service = new AuthService(
            userRepositoryMock.Object,
            jwtTokenServiceMock.Object);

        var request = new RegisterRequest
        {
            Name = "Test Customer",
            Email = "customer2@test.com",
            Password = "Password123!"
        };

        await service.RegisterAsync(request);

        Assert.NotNull(createdUser);

        Assert.NotEqual(
            request.Password,
            createdUser.PasswordHash);

        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                createdUser.PasswordHash));
    }
}
