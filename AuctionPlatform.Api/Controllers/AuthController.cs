using AuctionPlatform.Api.Models;
using AuctionPlatform.Application.DTOs;
using AuctionPlatform.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuctionPlatform.Api.Controllers;

/// <summary>
/// Controlador de autenticación. Maneja registro y login de usuarios.
/// Todos los endpoints son públicos (no requieren autenticación).
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Crea una nueva instancia del controlador de autenticación.
    /// </summary>
    /// <param name="authService">Servicio de autenticación para registro y login.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registra un nuevo usuario con rol Comprador y monedero inicial en cero.
    /// </summary>
    /// <param name="dto">Datos de registro: nombre, email y contraseña.</param>
    /// <returns>HTTP 201 con token JWT y datos de sesión.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        var response = new ApiResponse<AuthResult>(result, null, null);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Autentica un usuario y retorna un token JWT.
    /// Retorna HTTP 401 con mensaje genérico si las credenciales son inválidas o el usuario está bloqueado.
    /// </summary>
    /// <param name="dto">Datos de login: email y contraseña.</param>
    /// <returns>HTTP 200 con token JWT y datos de sesión.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        var response = new ApiResponse<AuthResult>(result, null, null);
        return Ok(response);
    }
}
