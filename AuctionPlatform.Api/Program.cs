using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AuctionPlatform.Api.Middleware;
using AuctionPlatform.Application.Services;
using AuctionPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Configuración de controladores con opciones de serialización JSON ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // camelCase para nombres de propiedades en las respuestas JSON
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        // Serializar enums como texto legible en lugar de valores numéricos
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // No omitir campos null: data, error, meta siempre deben estar presentes
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    });

// --- Configuración de Swagger/OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Plataforma de Subastas API",
        Version = "v1",
        Description = "API REST para la plataforma de subastas en tiempo real."
    });
});

// --- Caché distribuido en memoria (en desarrollo; en producción se reemplaza por Redis) ---
builder.Services.AddDistributedMemoryCache();

// --- Configuración de autenticación JWT ---
// JWT Bearer es el esquema de autenticación estándar para APIs REST stateless.
// Cada request incluye un token en el header Authorization que se valida automáticamente.
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret no está configurado en appsettings.json");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AuctionPlatform";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AuctionPlatform";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validar la firma del token con la clave secreta
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

        // Validar emisor y audiencia para prevenir uso de tokens de otros sistemas
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        // Validar que el token no haya expirado
        ValidateLifetime = true,

        // Sin tolerancia de reloj para expiración estricta
        ClockSkew = TimeSpan.Zero
    };
});

// --- Configuración de autorización ---
builder.Services.AddAuthorization();

// --- Registro del servicio JWT (scoped para inyección en servicios de autenticación) ---
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// --- Registro del servicio de autenticación (registro, login, bloqueo) ---
builder.Services.AddScoped<IAuthService, AuthService>();

// --- Configuración de Rate Limiting (100 req/min por usuario JWT o IP) ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("PerUser", httpContext =>
    {
        // Identificar al usuario por claim 'sub' del JWT o por IP si es anónimo
        var userId = httpContext.User?.FindFirst("sub")?.Value
                     ?? httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? httpContext.Connection.RemoteIpAddress?.ToString()
                     ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,            // 100 solicitudes máximo
            Window = TimeSpan.FromMinutes(1), // ventana de 1 minuto
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0                // sin cola, rechazo inmediato al exceder
        });
    });

    // Política global como fallback
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var userId = httpContext.User?.FindFirst("sub")?.Value
                     ?? httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? httpContext.Connection.RemoteIpAddress?.ToString()
                     ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});

var app = builder.Build();

// --- Middleware de excepciones: debe ir primero para capturar todo ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

// --- Swagger solo disponible en entorno de desarrollo ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Configurar Swagger UI en /api/docs
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Plataforma de Subastas API v1");
        options.RoutePrefix = "api/docs";
    });
}

// --- Rate limiting ---
app.UseRateLimiter();

// --- Autenticación y autorización JWT ---
app.UseAuthentication();
app.UseAuthorization();

// --- Middleware de lista negra: rechaza con 403 a usuarios bloqueados ---
// Se ejecuta DESPUÉS de la autenticación para tener acceso a los claims del usuario
app.UseMiddleware<TokenBlacklistMiddleware>();

// --- Mapear controladores bajo el prefijo /api/v1 ---
app.MapControllers().RequireRateLimiting("PerUser");

app.Run();
