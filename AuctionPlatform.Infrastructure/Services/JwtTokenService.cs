using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuctionPlatform.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuctionPlatform.Infrastructure.Services;

/// <summary>
/// Implementación del servicio JWT que genera tokens firmados con HS256.
/// Lee la configuración (Secret, Issuer, Audience, ExpirationHours) desde appsettings.json.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Crea una nueva instancia del servicio JWT.
    /// </summary>
    /// <param name="configuration">Configuración de la aplicación para leer la sección "Jwt".</param>
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public string GenerateToken(int userId, string role)
    {
        // Leer configuración JWT desde appsettings.json
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("La clave secreta JWT no está configurada en appsettings.json (Jwt:Secret).");
        var issuer = _configuration["Jwt:Issuer"] ?? "AuctionPlatform";
        var audience = _configuration["Jwt:Audience"] ?? "AuctionPlatform";
        var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "24");

        // Crear la clave simétrica para firmar el token con HS256
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Claims del token: 'sub' identifica al usuario, 'role' define su rol en el sistema
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Crear el token con los parámetros configurados
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
