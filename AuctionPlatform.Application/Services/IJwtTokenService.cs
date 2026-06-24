namespace AuctionPlatform.Application.Services;

/// <summary>
/// Servicio para la generación de tokens JWT de autenticación.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Genera un token JWT firmado con los claims del usuario.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="role">Nombre del rol del usuario (ej: "Comprador", "Administrador").</param>
    /// <returns>Token JWT como cadena codificada en Base64.</returns>
    string GenerateToken(int userId, string role);
}
