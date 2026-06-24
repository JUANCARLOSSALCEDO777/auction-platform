using AuctionPlatform.Application.DTOs;
using AuctionPlatform.Application.Services;
using AuctionPlatform.Application.Validators;
using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.Exceptions;
using AuctionPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuctionPlatform.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de autenticación.
/// Maneja registro, login, perfil y actualización de datos del usuario.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AuctionDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Crea una nueva instancia del servicio de autenticación.
    /// </summary>
    /// <param name="context">Contexto de base de datos EF Core.</param>
    /// <param name="jwtTokenService">Servicio de generación de tokens JWT.</param>
    /// <param name="logger">Logger para registrar eventos del servicio.</param>
    public AuthService(
        AuctionDbContext context,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        // 1. Validar datos de entrada (lanza ValidationException si hay errores)
        UserValidator.ValidateRegistration(dto);

        // 2. Verificar que el email no esté ya registrado
        var emailExists = await _context.Usuarios
            .AnyAsync(u => u.Email == dto.Email.ToLowerInvariant());

        if (emailExists)
        {
            // Mensaje genérico para no revelar si el email existe (seguridad)
            throw new ConflictException("No se pudo completar el registro. Intente nuevamente.");
        }

        // 3. Hashear la contraseña con bcrypt (cost factor 12 para balance seguridad/rendimiento)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

        // 4. Crear la entidad Usuario con rol Comprador (IdRol=2) y estado activo
        var usuario = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Email = dto.Email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            IdRol = 2, // Rol "Comprador" según seed de datos
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);

        // 5. Crear el monedero asociado con saldos en cero
        var monedero = new Monedero
        {
            Usuario = usuario,
            SaldoDisponible = 0.00m,
            SaldoComprometido = 0.00m
        };

        _context.Monederos.Add(monedero);

        // 6. Persistir usuario y monedero en una sola transacción
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario registrado exitosamente: {Email}", usuario.Email);

        // 7. Generar token JWT con el id del nuevo usuario y su rol
        var token = _jwtTokenService.GenerateToken(usuario.Id, "Comprador");
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new AuthResult(token, expiresAt, "Comprador");
    }

    /// <inheritdoc />
    public async Task<AuthResult> LoginAsync(LoginDto dto)
    {
        // 1. Buscar usuario por email (case-insensitive) incluyendo su rol
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLowerInvariant());

        // 2. Si no existe o está bloqueado → mensaje genérico para no revelar información
        if (usuario is null || !usuario.Activo)
        {
            throw new UnauthorizedException("Credenciales inválidas.");
        }

        // 3. Verificar contraseña con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
        {
            throw new UnauthorizedException("Credenciales inválidas.");
        }

        _logger.LogInformation("Login exitoso para usuario: {Email}", usuario.Email);

        // 4. Generar token JWT con id y rol del usuario
        var roleName = usuario.Rol.Nombre;
        var token = _jwtTokenService.GenerateToken(usuario.Id, roleName);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new AuthResult(token, expiresAt, roleName);
    }

    /// <inheritdoc />
    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        // Cargar usuario con su rol para obtener el nombre del rol
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (usuario is null)
        {
            throw new NotFoundException($"Usuario con id {userId} no encontrado.");
        }

        return new UserProfileDto(
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            usuario.Rol.Nombre,
            usuario.FechaCreacion
        );
    }

    /// <inheritdoc />
    public async Task UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        // 1. Cargar usuario existente
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (usuario is null)
        {
            throw new NotFoundException($"Usuario con id {userId} no encontrado.");
        }

        // 2. Validar y actualizar nombre si se proporcionó
        if (!string.IsNullOrWhiteSpace(dto.Nombre))
        {
            UserValidator.ValidateProfileName(dto.Nombre);
            usuario.Nombre = dto.Nombre.Trim();
        }

        // 3. Validar y actualizar contraseña si se proporcionó
        if (!string.IsNullOrEmpty(dto.Password))
        {
            UserValidator.ValidateProfilePassword(dto.Password);
            // Re-hashear con bcrypt cost 12
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);
        }

        // 4. IGNORAR dto.Email completamente — el correo electrónico no es modificable

        // 5. Persistir cambios
        await _context.SaveChangesAsync();

        _logger.LogInformation("Perfil actualizado para usuario: {UserId}", userId);
    }
}
