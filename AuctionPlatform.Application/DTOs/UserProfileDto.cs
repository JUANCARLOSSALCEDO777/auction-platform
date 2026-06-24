namespace AuctionPlatform.Application.DTOs;

/// <summary>
/// Datos del perfil del usuario autenticado para respuestas de la API.
/// </summary>
/// <param name="Id">Identificador único del usuario.</param>
/// <param name="Nombre">Nombre del usuario.</param>
/// <param name="Email">Correo electrónico del usuario.</param>
/// <param name="Rol">Nombre del rol asignado (Comprador o Administrador).</param>
/// <param name="FechaCreacion">Fecha y hora UTC de creación de la cuenta.</param>
public record UserProfileDto(int Id, string Nombre, string Email, string Rol, DateTime FechaCreacion);
