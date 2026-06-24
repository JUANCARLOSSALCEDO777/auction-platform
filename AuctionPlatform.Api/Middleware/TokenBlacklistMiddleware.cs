using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;

namespace AuctionPlatform.Api.Middleware;

/// <summary>
/// Middleware que valida si un usuario autenticado está en la lista negra de tokens.
/// Consulta IDistributedCache con la clave "blacklist:{userId}".
/// Si el usuario está bloqueado, retorna HTTP 403 Forbidden.
/// Solo se evalúa para requests autenticados; las solicitudes anónimas pasan sin verificación.
/// </summary>
public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Crea una nueva instancia del middleware de lista negra.
    /// </summary>
    /// <param name="next">Siguiente delegado en el pipeline de la aplicación.</param>
    public TokenBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Evalúa si el usuario autenticado está en la lista negra antes de continuar el pipeline.
    /// </summary>
    /// <param name="context">Contexto HTTP de la solicitud actual.</param>
    /// <param name="cache">Servicio de caché distribuido para consultar la lista negra.</param>
    public async Task InvokeAsync(HttpContext context, IDistributedCache cache)
    {
        // Solo verificar para usuarios autenticados
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Obtener el ID del usuario desde el claim 'sub' del JWT
            var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Verificar si el usuario está en la lista negra (clave: "blacklist:{userId}")
                var blacklisted = await cache.GetStringAsync($"blacklist:{userId}");

                if (blacklisted != null)
                {
                    // El usuario está bloqueado: retornar 403 sin continuar el pipeline
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        """{"data":null,"error":"Usuario bloqueado. Acceso denegado.","meta":null}""");
                    return;
                }
            }
        }

        // Usuario no bloqueado o solicitud anónima: continuar con el pipeline
        await _next(context);
    }
}
