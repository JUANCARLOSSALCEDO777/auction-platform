using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuctionPlatform.Infrastructure.Data;

/// <summary>
/// Factoría de DbContext para uso exclusivo de las herramientas de EF Core en tiempo de diseño
/// (migraciones, scaffolding). Permite ejecutar comandos como dotnet ef migrations add
/// sin necesidad de un host ASP.NET Core en ejecución.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuctionDbContext>
{
    /// <summary>
    /// Crea una instancia del DbContext configurada con SQL Server para uso en diseño.
    /// </summary>
    /// <param name="args">Argumentos de línea de comandos (no utilizados).</param>
    /// <returns>Instancia de <see cref="AuctionDbContext"/> configurada.</returns>
    public AuctionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuctionDbContext>();

        // Connection string por defecto para migraciones en desarrollo
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=AuctionPlatform;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true");

        return new AuctionDbContext(optionsBuilder.Options);
    }
}
