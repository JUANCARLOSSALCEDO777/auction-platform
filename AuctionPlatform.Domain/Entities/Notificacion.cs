namespace AuctionPlatform.Domain.Entities;

public class Notificacion
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public bool Leido { get; set; }
    public DateTime Fecha { get; set; }

    // Navegación
    public Usuario Usuario { get; set; } = null!;
}
