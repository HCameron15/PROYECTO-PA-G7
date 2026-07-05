using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.Models;
using Microsoft.EntityFrameworkCore;
using Uam.AdvancedProgramming.Api.Constants;

namespace Uam.AdvancedProgramming.Api.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        var adminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == "Admin");

        if (adminRole is null)
        {
            adminRole = new Role
            {
                Name = "Admin",
                Description = "Administrator role",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await context.Roles.AddAsync(adminRole);
            await context.SaveChangesAsync();
        }

        var userRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == "User");

        if (userRole is null)
        {
            userRole = new Role
            {
                Name = "User",
                Description = "Standard user role",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await context.Roles.AddAsync(userRole);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync(u => u.Email == "admin@uam.com"))
        {
            var adminUser = new User
            {
                FirstName = "System",
                LastName = "Admin",
                Email = "admin@uam.com",
                RoleId = adminRole.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }
}