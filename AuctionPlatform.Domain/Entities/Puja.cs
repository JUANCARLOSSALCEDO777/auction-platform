namespace AuctionPlatform.Domain.Entities;

public class Puja
{
    public int Id { get; set; }
    public int IdSubasta { get; set; }
    public int IdUsuario { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaHora { get; set; }

    // Navegación
    public Subasta Subasta { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
