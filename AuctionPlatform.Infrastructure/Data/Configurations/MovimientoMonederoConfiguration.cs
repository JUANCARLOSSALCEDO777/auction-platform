using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Data.Configurations;

public class MovimientoMonederoConfiguration : IEntityTypeConfiguration<MovimientoMonedero>
{
    public void Configure(EntityTypeBuilder<MovimientoMonedero> builder)
    {
        builder.ToTable("MovimientosMonedero");

        builder.HasKey(mm => mm.Id);

        builder.Property(mm => mm.Tipo)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(mm => mm.Monto)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(mm => mm.Fecha)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(mm => mm.IdUsuarioAdmin)
            .IsRequired(false);

        // Relación con Monedero
        builder.HasOne(mm => mm.Monedero)
            .WithMany(m => m.Movimientos)
            .HasForeignKey(mm => mm.IdMonedero)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
