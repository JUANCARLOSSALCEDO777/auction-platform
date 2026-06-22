using AuctionPlatform.Domain.Enums;

namespace AuctionPlatform.Domain.Entities;

public class Subasta
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PrecioInicial { get; set; }
    public decimal PujaMinima { get; set; }
    public decimal IncrementoMinimo { get; set; }
    public EstadoSubasta Estado { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int IdAdminCreador { get; set; }
    public int? IdUsuarioGanador { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    // Navegación
    public Usuario AdminCreador { get; set; } = null!;
    public Usuario? UsuarioGanador { get; set; }
    public ICollection<Puja> Pujas { get; set; } = new List<Puja>();
    public ICollection<Imagen> Imagenes { get; set; } = new List<Imagen>();
}
