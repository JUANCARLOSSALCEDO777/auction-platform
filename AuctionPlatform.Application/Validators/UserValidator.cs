using System.Text.RegularExpressions;
using AuctionPlatform.Application.DTOs;
using AuctionPlatform.Domain.Exceptions;

namespace AuctionPlatform.Application.Validators;

/// <summary>
/// Validador estático para datos de registro de usuario.
/// Aplica las reglas de negocio: nombre (2-100 chars), email (RFC 5321), contraseña (8-128 chars con mayúscula, minúscula y dígito).
/// </summary>
public static class UserValidator
{
    // Regex simplificado para validación de email según RFC 5321:
    // local-part@domain donde local-part permite alfanuméricos y caracteres especiales comunes,
    // y domain permite subdominios con al menos un punto y TLD de 2+ caracteres.
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    /// <summary>
    /// Valida los datos de registro de un usuario.
    /// Lanza <see cref="ValidationException"/> si algún campo no cumple las reglas.
    /// </summary>
    /// <param name="dto">Datos de registro a validar.</param>
    /// <exception cref="ValidationException">Si uno o más campos son inválidos.</exception>
    public static void ValidateRegistration(RegisterDto dto)
    {
        var errors = new Dictionary<string, List<string>>();

        // Validación de nombre: 2-100 caracteres
        ValidateName(dto.Nombre, errors);

        // Validación de email: formato RFC 5321, máximo 254 caracteres
        ValidateEmail(dto.Email, errors);

        // Validación de contraseña: 8-128 chars, al menos 1 mayúscula, 1 minúscula, 1 dígito
        ValidatePassword(dto.Password, errors);

        if (errors.Count > 0)
        {
            var formattedErrors = errors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray());

            throw new ValidationException(formattedErrors);
        }
    }

    private static void ValidateName(string? nombre, Dictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            AddError(errors, "nombre", "El nombre es obligatorio.");
            return;
        }

        if (nombre.Length < 2)
            AddError(errors, "nombre", "El nombre debe tener al menos 2 caracteres.");

        if (nombre.Length > 100)
            AddError(errors, "nombre", "El nombre no puede exceder 100 caracteres.");
    }

    private static void ValidateEmail(string? email, Dictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            AddError(errors, "email", "El email es obligatorio.");
            return;
        }

        // max 254 caracteres según RFC 5321
        if (email.Length > 254)
        {
            AddError(errors, "email", "El email no puede exceder 254 caracteres.");
            return;
        }

        if (!EmailRegex.IsMatch(email))
            AddError(errors, "email", "El formato del email no es válido.");
    }

    private static void ValidatePassword(string? password, Dictionary<string, List<string>> errors)
    {
        if (string.IsNullOrEmpty(password))
        {
            AddError(errors, "password", "La contraseña es obligatoria.");
            return;
        }

        if (password.Length < 8)
            AddError(errors, "password", "La contraseña debe tener al menos 8 caracteres.");

        if (password.Length > 128)
            AddError(errors, "password", "La contraseña no puede exceder 128 caracteres.");

        if (!password.Any(char.IsUpper))
            AddError(errors, "password", "La contraseña debe contener al menos una letra mayúscula.");

        if (!password.Any(char.IsLower))
            AddError(errors, "password", "La contraseña debe contener al menos una letra minúscula.");

        if (!password.Any(char.IsDigit))
            AddError(errors, "password", "La contraseña debe contener al menos un dígito.");
    }

    /// <summary>
    /// Valida el nombre para actualización de perfil (2-100 caracteres).
    /// Lanza <see cref="ValidationException"/> si no cumple las reglas.
    /// </summary>
    /// <param name="nombre">Nombre a validar.</param>
    /// <exception cref="ValidationException">Si el nombre es inválido.</exception>
    public static void ValidateProfileName(string nombre)
    {
        var errors = new Dictionary<string, List<string>>();
        ValidateName(nombre, errors);

        if (errors.Count > 0)
        {
            var formattedErrors = errors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray());

            throw new ValidationException(formattedErrors);
        }
    }

    /// <summary>
    /// Valida la contraseña para actualización de perfil (8-128 chars, mayúscula, minúscula, dígito).
    /// Lanza <see cref="ValidationException"/> si no cumple las reglas.
    /// </summary>
    /// <param name="password">Contraseña a validar.</param>
    /// <exception cref="ValidationException">Si la contraseña es inválida.</exception>
    public static void ValidateProfilePassword(string password)
    {
        var errors = new Dictionary<string, List<string>>();
        ValidatePassword(password, errors);

        if (errors.Count > 0)
        {
            var formattedErrors = errors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray());

            throw new ValidationException(formattedErrors);
        }
    }

    private static void AddError(Dictionary<string, List<string>> errors, string field, string message)
    {
        if (!errors.ContainsKey(field))
            errors[field] = new List<string>();

        errors[field].Add(message);
    }
}
