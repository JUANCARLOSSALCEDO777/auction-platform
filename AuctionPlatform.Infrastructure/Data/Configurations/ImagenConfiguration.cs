using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Data.Configurations;

public class ImagenConfiguration : IEntityTypeConfiguration<Imagen>
{
    public void Configure(EntityTypeBuilder<Imagen> builder)
    {
        builder.ToTable("Imagenes");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.UrlAlmacenamiento)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.Orden)
            .IsRequired();

        // Relación con Subasta
        builder.HasOne(i => i.Subasta)
            .WithMany(s => s.Imagenes)
            .HasForeignKey(i => i.IdSubasta)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
