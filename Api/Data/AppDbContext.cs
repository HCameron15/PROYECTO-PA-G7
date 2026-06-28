using Microsoft.EntityFrameworkCore;
using Uam.AdvancedProgramming.Api.Models;

namespace Uam.AdvancedProgramming.Api.Data;

/// <summary>
/// Contexto principal de Entity Framework Core para la API.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Tabla lógica de estudiantes.
    /// </summary>
    public DbSet<Student> Students => Set<Student>();

    /// <summary>
    /// Tabla lógica de laboratorios.
    /// </summary>
    public DbSet<Laboratory> Laboratories => Set<Laboratory>();

    /// <summary>
    /// Tabla lógica de equipos.
    /// </summary>
    public DbSet<Equipment> Equipment => Set<Equipment>();

    /// <summary>
    /// Configuración del modelo de datos (tabla, llaves, índices y restricciones).
    /// </summary>
    /// /// <summary>
    /// Tabla lógica de roles.
    /// </summary>
    public DbSet<Role> Roles => Set<Role>();

    /// <summary>
    /// Tabla lógica de usuarios.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

    public DbSet<PendingLoginSession> PendingLoginSessions => Set<PendingLoginSession>();

    public DbSet<PasswordResetRequest> PasswordResetRequests => Set<PasswordResetRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordResetRequest>(entity =>
        {
            entity.ToTable("PasswordResetRequests");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.SessionToken)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.SessionToken)
                .IsUnique();

            entity.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(x => x.ExpiresAtUtc)
                .IsRequired();

            entity.Property(x => x.IsUsed)
                .HasDefaultValue(false);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.Property(x => x.UsedAtUtc)
                .IsRequired(false);

            entity.HasOne(x => x.User)
                .WithMany(x => x.PasswordResetRequests)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.BirthDate).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<Laboratory>(entity =>
        {
            entity.ToTable("Laboratories");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.Name)
                .IsUnique();

            entity.Property(x => x.Building)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Floor)
                .IsRequired();

            entity.Property(x => x.Capacity)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.Property(x => x.UpdatedAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.ToTable("Equipment");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LaboratoryId)
                .IsRequired();

            entity.Property(x => x.Code)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.Property(x => x.Brand)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Model)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.SerialNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.SerialNumber)
                .IsUnique();

            entity.Property(x => x.Type)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(20)
                .IsRequired();



            entity.Property(x => x.PurchaseDate)
                .HasColumnType("date");

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.Property(x => x.UpdatedAtUtc)
                .IsRequired();

            entity.HasOne(x => x.Laboratory)
                .WithMany(x => x.Equipment)
                .HasForeignKey(x => x.LaboratoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.Name)
                .IsUnique();

            entity.Property(x => x.Description)
                .HasMaxLength(200);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.Property(x => x.UpdatedAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.RoleId)
                .IsRequired();

            entity.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.Property(x => x.UpdatedAtUtc)
                .IsRequired();

            entity.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.Property(x => x.Token)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasIndex(x => x.Token)
                .IsUnique();

            entity.Property(x => x.ExpiresAtUtc)
                .IsRequired();

            entity.Property(x => x.IsRevoked)
                .HasDefaultValue(false);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(x => x.RevokedAtUtc)
                .IsRequired(false);

            entity.Property(x => x.RevokedReason)
                .HasMaxLength(200)
                .IsRequired(false);
        });

        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.ToTable("OtpCodes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.Property(x => x.Code)
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(x => x.ExpiresAtUtc)
                .IsRequired();

            entity.Property(x => x.IsUsed)
                .HasDefaultValue(false);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.OtpCodes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PendingLoginSession>(entity =>
        {
            entity.ToTable("PendingLoginSessions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.Property(x => x.SessionToken)
                .IsRequired();

            entity.HasIndex(x => x.SessionToken)
                .IsUnique();

            entity.Property(x => x.ExpiresAtUtc)
                .IsRequired();

            entity.Property(x => x.IsUsed)
                .HasDefaultValue(false);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.PendingLoginSessions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}