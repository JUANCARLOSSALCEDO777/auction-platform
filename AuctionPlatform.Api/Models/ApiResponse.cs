namespace AuctionPlatform.Api.Models;

/// <summary>
/// Envoltura estándar para todas las respuestas de la API.
/// Garantiza que toda respuesta contenga los campos data, error y meta,
/// incluso cuando sus valores sean null.
/// </summary>
/// <typeparam name="T">Tipo del payload de datos.</typeparam>
public record ApiResponse<T>(T? Data, string? Error, object? Meta);
