namespace AuctionPlatform.Application.DTOs;

/// <summary>
/// Datos requeridos para iniciar sesión en la plataforma.
/// </summary>
/// <param name="Email">Correo electrónico del usuario.</param>
/// <param name="Password">Contraseña del usuario.</param>
public record LoginDto(string Email, string Password);
