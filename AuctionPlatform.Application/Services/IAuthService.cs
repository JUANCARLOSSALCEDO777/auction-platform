using AuctionPlatform.Application.DTOs;

namespace AuctionPlatform.Application.Services;

/// <summary>
/// Servicio de autenticación que maneja registro, login y gestión de usuarios.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registra un nuevo usuario con rol Comprador y monedero inicial en cero.
    /// </summary>
    /// <param name="dto">Datos de registro: nombre, email y contraseña.</param>
    /// <returns>Token JWT y datos de sesión.</returns>
    /// <exception cref="Domain.Exceptions.ConflictException">Si el email ya está registrado.</exception>
    /// <exception cref="Domain.Exceptions.ValidationException">Si los datos no pasan validación.</exception>
    Task<AuthResult> RegisterAsync(RegisterDto dto);
}
