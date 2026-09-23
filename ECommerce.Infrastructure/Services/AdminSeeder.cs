using BCrypt.Net;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Services;

public class AdminSeeder
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AdminSeeder(
        AppDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var adminEmail = _configuration["Admin:Email"];
        var adminPassword = _configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var existingAdmin = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (existingAdmin is not null)
        {
            return;
        }

        var admin = new User
        {
            Name = "System Admin",
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                adminPassword),
            Role = Roles.Admin,
            IsActive = true
        };

        await _context.Users.AddAsync(admin);
        await _context.SaveChangesAsync();
    }
}