using Uam.AdvancedProgramming.Api.Data;
using Uam.AdvancedProgramming.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Uam.AdvancedProgramming.Api.Data.Seed;

/// <summary>
/// Inicializa datos base del sistema (roles y usuario administrador).
/// Se ejecuta al iniciar la aplicación.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Ejecuta el seed inicial de la base de datos.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context)
    {
        // Asegura que la base de datos esté creada
        await context.Database.MigrateAsync();

        // =========================
        // 1. CREAR ROLES BASE
        // =========================
        if (!await context.Roles.AnyAsync())
        {
            var adminRole = new Role
            {
                Name = "Admin",
                Description = "Administrator role",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            var userRole = new Role
            {
                Name = "User",
                Description = "Standard user role",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await context.Roles.AddRangeAsync(adminRole, userRole);
            await context.SaveChangesAsync();
        }

        // =========================
        // 2. CREAR USUARIO ADMIN
        // =========================

        if (!await context.Users.AnyAsync(u => u.Email == "admin@uam.com"))
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");

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