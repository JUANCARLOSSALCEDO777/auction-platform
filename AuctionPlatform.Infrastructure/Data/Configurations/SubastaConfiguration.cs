using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Data.Configurations;

public class SubastaConfiguration : IEntityTypeConfiguration<Subasta>
{
    public void Configure(EntityTypeBuilder<Subasta> builder)
    {
        builder.ToTable("Subastas");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Titulo)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Descripcion)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(s => s.Categoria)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.PrecioInicial)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.PujaMinima)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.IncrementoMinimo)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.FechaInicio)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(s => s.FechaFin)
            .HasColumnType("datetime2")
            .IsRequired();

        // Concurrencia optimista con rowversion
        builder.Property(s => s.RowVersion)
            .IsRowVersion();

        // Relación con AdminCreador
        builder.HasOne(s => s.AdminCreador)
            .WithMany()
            .HasForeignKey(s => s.IdAdminCreador)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con UsuarioGanador (opcional)
        builder.HasOne(s => s.UsuarioGanador)
            .WithMany()
            .HasForeignKey(s => s.IdUsuarioGanador)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
