using BCrypt.Net;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Application.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IRepository<User> userRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        var existingUser = await _userRepository
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser is not null)
        {
            throw new BusinessException(
                "A user with this email already exists.");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password),
            Role = Roles.Customer,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }


    public async Task<AuthResponse> CreateAdminAsync(
    CreateAdminRequest request)
    {
        var existingUser = await _userRepository
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser is not null)
        {
            throw new BusinessException(
                "A user with this email already exists.");
        }

        var admin = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password),
            Role = Roles.Admin,
            IsActive = true
        };

        await _userRepository.AddAsync(admin);
        await _userRepository.SaveChangesAsync();

        var token = _jwtTokenService.GenerateToken(admin);

        return new AuthResponse
        {
            UserId = admin.Id,
            Name = admin.Name,
            Email = admin.Email,
            Role = admin.Role,
            Token = token
        };
    }

    public async Task<AuthResponse> LoginAsync(
    LoginRequest request)
    {
        var user = await _userRepository
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
        {
            throw new BusinessException(
                "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new BusinessException(
                "This account is inactive.");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new BusinessException(
                "Invalid email or password.");
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            Token = token
        };
    }
}