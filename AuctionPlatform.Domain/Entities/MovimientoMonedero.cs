using AuctionPlatform.Domain.Enums;

namespace AuctionPlatform.Domain.Entities;

public class MovimientoMonedero
{
    public int Id { get; set; }
    public int IdMonedero { get; set; }
    public TipoMovimiento Tipo { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public int? IdUsuarioAdmin { get; set; }

    // Navegación
    public Monedero Monedero { get; set; } = null!;
}
