namespace AuctionPlatform.Domain.Entities;

public class Monedero
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public decimal SaldoDisponible { get; set; }
    public decimal SaldoComprometido { get; set; }

    // Navegación
    public Usuario Usuario { get; set; } = null!;
    public ICollection<MovimientoMonedero> Movimientos { get; set; } = new List<MovimientoMonedero>();
}
