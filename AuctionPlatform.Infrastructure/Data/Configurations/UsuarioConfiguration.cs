using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(72)
            .IsRequired();

        builder.Property(u => u.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(u => u.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        // Relación con Rol
        builder.HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.IdRol)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con Monedero (1:1)
        builder.HasOne(u => u.Monedero)
            .WithOne(m => m.Usuario)
            .HasForeignKey<Monedero>(m => m.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
