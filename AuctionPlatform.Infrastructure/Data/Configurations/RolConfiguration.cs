using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad Rol.
/// Define la tabla, restricciones y datos iniciales (seed).
/// </summary>
public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre)
            .HasMaxLength(50)
            .IsRequired();

        // Seed de roles iniciales del sistema
        builder.HasData(
            new Rol { Id = 1, Nombre = "Administrador" },
            new Rol { Id = 2, Nombre = "Comprador" }
        );
    }
}
