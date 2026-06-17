using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Uam.AdvancedProgramming.Api.Data;

/// <summary>
/// Fábrica de diseño para permitir que comandos de Entity Framework (migrations) creen el DbContext.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Crea una instancia de AppDbContext para tiempo de diseño.
    /// </summary>
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=UamAdvancedProgrammingDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
    
        return new AppDbContext(optionsBuilder.Options);
    }
}

