using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Data.Configurations;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.ToTable("Notificaciones");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Tipo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Contenido)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(n => n.Leido)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.Fecha)
            .HasColumnType("datetime2")
            .IsRequired();

        // Relación con Usuario
        builder.HasOne(n => n.Usuario)
            .WithMany(u => u.Notificaciones)
            .HasForeignKey(n => n.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
