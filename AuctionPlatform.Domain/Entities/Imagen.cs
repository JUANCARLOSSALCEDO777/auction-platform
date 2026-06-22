namespace AuctionPlatform.Domain.Entities;

public class Imagen
{
    public int Id { get; set; }
    public int IdSubasta { get; set; }
    public string UrlAlmacenamiento { get; set; } = string.Empty;
    public int Orden { get; set; }

    // Navegación
    public Subasta Subasta { get; set; } = null!;
}
