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

    /// <summary>
    /// Autentica un usuario verificando credenciales y retorna un token JWT.
    /// </summary>
    /// <param name="dto">Datos de login: email y contraseña.</param>
    /// <returns>Token JWT y datos de sesión.</returns>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Si las credenciales son inválidas o el usuario está bloqueado.</exception>
    Task<AuthResult> LoginAsync(LoginDto dto);

    /// <summary>
    /// Obtiene el perfil del usuario autenticado.
    /// </summary>
    /// <param name="userId">Identificador del usuario autenticado.</param>
    /// <returns>Datos del perfil del usuario.</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Si el usuario no existe.</exception>
    Task<UserProfileDto> GetProfileAsync(int userId);

    /// <summary>
    /// Actualiza el perfil del usuario autenticado (nombre y/o contraseña).
    /// El campo email se ignora completamente aunque se envíe en la solicitud.
    /// </summary>
    /// <param name="userId">Identificador del usuario autenticado.</param>
    /// <param name="dto">Datos a actualizar: nombre y/o contraseña (email se ignora).</param>
    /// <exception cref="Domain.Exceptions.ValidationException">Si los datos no pasan validación.</exception>
    /// <exception cref="Domain.Exceptions.NotFoundException">Si el usuario no existe.</exception>
    Task UpdateProfileAsync(int userId, UpdateProfileDto dto);
}
