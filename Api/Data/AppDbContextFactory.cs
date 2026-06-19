using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Uam.AdvancedProgramming.Api.Data;

/// <summary>
/// Fábrica de diseño para permitir que comandos de Entity Framework (migrations)
/// creen el DbContext en tiempo de diseño.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Crea una instancia de AppDbContext para tiempo de diseño.
    /// Esto es utilizado por los comandos de EF Core como:
    /// Add-Migration y Update-Database.
    /// </summary>
    public AppDbContext CreateDbContext(string[] args)
    {
        // Construimos la configuración leyendo el appsettings.json del proyecto
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Obtenemos la cadena de conexión desde el archivo de configuración
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Configuramos las opciones del DbContext para SQL Server
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        // Retornamos la instancia configurada del DbContext
        return new AppDbContext(optionsBuilder.Options);
    }
}