namespace AuctionPlatform.Application.DTOs;

/// <summary>
/// Resultado de autenticación exitosa (registro o login).
/// </summary>
/// <param name="Token">Token JWT firmado.</param>
/// <param name="ExpiresAt">Fecha y hora UTC de expiración del token.</param>
/// <param name="Rol">Nombre del rol asignado al usuario.</param>
public record AuthResult(string Token, DateTime ExpiresAt, string Rol);
