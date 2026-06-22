using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Data.Configurations;

public class MonederoConfiguration : IEntityTypeConfiguration<Monedero>
{
    public void Configure(EntityTypeBuilder<Monedero> builder)
    {
        builder.ToTable("Monederos");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.SaldoDisponible)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(m => m.SaldoComprometido)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Índice único en IdUsuario (relación 1:1)
        builder.HasIndex(m => m.IdUsuario)
            .IsUnique();
    }
}
