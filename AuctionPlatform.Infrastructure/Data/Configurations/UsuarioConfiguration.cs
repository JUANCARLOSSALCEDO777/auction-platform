using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad Usuario.
/// Define la tabla, restricciones, índices, relaciones y datos iniciales (seed).
/// </summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        // max 254 chars según RFC 5321
        builder.Property(u => u.Email)
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        // bcrypt genera hashes de hasta 60 caracteres, se usa 72 para margen
        builder.Property(u => u.PasswordHash)
            .HasMaxLength(72)
            .IsRequired();

        builder.Property(u => u.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(u => u.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        // Relación con Rol — Restrict para no eliminar roles con usuarios asignados
        builder.HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.IdRol)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con Monedero (1:1) — Cascade para eliminar monedero si se elimina usuario
        builder.HasOne(u => u.Monedero)
            .WithOne(m => m.Usuario)
            .HasForeignKey<Monedero>(m => m.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed del usuario administrador inicial
        // Hash bcrypt pre-computado para "Admin123!" con cost 12
        builder.HasData(new Usuario
        {
            Id = 1,
            Nombre = "Admin",
            Email = "admin@subastas.com",
            PasswordHash = "$2a$12$LQv3c1yqBo9SkvXS7QTJPOeGxVdRZL7.GnXlJY1bVIYkh5JlsD1Vy",
            FechaCreacion = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Activo = true,
            IdRol = 1 // Administrador
        });
    }
}
