namespace AuctionPlatform.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; } = true;
    public int IdRol { get; set; }

    // Navegación
    public Rol Rol { get; set; } = null!;
    public Monedero Monedero { get; set; } = null!;
    public ICollection<Puja> Pujas { get; set; } = new List<Puja>();
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
}
