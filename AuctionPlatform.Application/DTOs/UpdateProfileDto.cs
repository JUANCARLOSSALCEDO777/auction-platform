namespace AuctionPlatform.Application.DTOs;

/// <summary>
/// Datos para actualización de perfil del usuario autenticado.
/// El campo Email se incluye por compatibilidad pero se ignora en el procesamiento.
/// </summary>
/// <param name="Nombre">Nuevo nombre del usuario (2-100 caracteres). Null si no se desea cambiar.</param>
/// <param name="Password">Nueva contraseña (8-128 chars, mayúscula, minúscula, dígito). Null si no se desea cambiar.</param>
/// <param name="Email">Campo ignorado. No se permite modificar el correo electrónico.</param>
public record UpdateProfileDto(string? Nombre, string? Password, string? Email);
