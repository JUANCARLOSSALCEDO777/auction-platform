namespace AuctionPlatform.Application.DTOs;

/// <summary>
/// Datos requeridos para registrar un nuevo usuario en la plataforma.
/// </summary>
/// <param name="Nombre">Nombre del usuario (2-100 caracteres).</param>
/// <param name="Email">Correo electrónico en formato RFC 5321.</param>
/// <param name="Password">Contraseña (8-128 chars, al menos 1 mayúscula, 1 minúscula, 1 dígito).</param>
public record RegisterDto(string Nombre, string Email, string Password);
