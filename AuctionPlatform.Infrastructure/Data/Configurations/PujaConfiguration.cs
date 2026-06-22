using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Data.Configurations;

public class PujaConfiguration : IEntityTypeConfiguration<Puja>
{
    public void Configure(EntityTypeBuilder<Puja> builder)
    {
        builder.ToTable("Pujas");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Monto)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.FechaHora)
            .HasColumnType("datetime2")
            .IsRequired();

        // Índice compuesto para desempate eficiente
        builder.HasIndex(p => new { p.IdSubasta, p.Monto, p.FechaHora });

        // Relación con Subasta
        builder.HasOne(p => p.Subasta)
            .WithMany(s => s.Pujas)
            .HasForeignKey(p => p.IdSubasta)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con Usuario
        builder.HasOne(p => p.Usuario)
            .WithMany(u => u.Pujas)
            .HasForeignKey(p => p.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
