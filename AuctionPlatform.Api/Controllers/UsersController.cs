using System.Security.Claims;
using AuctionPlatform.Api.Models;
using AuctionPlatform.Application.DTOs;
using AuctionPlatform.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionPlatform.Api.Controllers;

/// <summary>
/// Controlador de gestión de perfil de usuario autenticado.
/// Todos los endpoints requieren autenticación JWT.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Crea una nueva instancia del controlador de usuarios.
    /// </summary>
    /// <param name="authService">Servicio de autenticación para consulta y actualización de perfil.</param>
    public UsersController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado.
    /// </summary>
    /// <returns>HTTP 200 con datos del perfil.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetAuthenticatedUserId();
        var profile = await _authService.GetProfileAsync(userId);

        var response = new ApiResponse<UserProfileDto>(profile, null, null);
        return Ok(response);
    }

    /// <summary>
    /// Actualiza el perfil del usuario autenticado (nombre y/o contraseña).
    /// El campo email se ignora completamente aunque se incluya en el cuerpo.
    /// </summary>
    /// <param name="dto">Datos a actualizar: nombre y/o contraseña.</param>
    /// <returns>HTTP 200 con confirmación.</returns>
    [HttpPut("me")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetAuthenticatedUserId();
        await _authService.UpdateProfileAsync(userId, dto);

        var response = new ApiResponse<object>(new { message = "Perfil actualizado exitosamente." }, null, null);
        return Ok(response);
    }

    /// <summary>
    /// Extrae el ID del usuario autenticado desde los claims del token JWT.
    /// El claim "sub" contiene el identificador numérico del usuario.
    /// </summary>
    private int GetAuthenticatedUserId()
    {
        // El claim "sub" (subject) del JWT contiene el ID del usuario
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new Domain.Exceptions.UnauthorizedException("Token inválido: no se pudo identificar al usuario.");
        }

        return userId;
    }
}
