using System.Net;
using System.Text.Json;
using AuctionPlatform.Api.Models;
using AuctionPlatform.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Api.Middleware;

/// <summary>
/// Middleware global de manejo de excepciones. Intercepta todas las excepciones
/// no controladas y las mapea a respuestas HTTP estructuradas con <see cref="ApiResponse{T}"/>.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Inicializa una nueva instancia del middleware de excepciones.
    /// </summary>
    /// <param name="next">Delegado del siguiente middleware en el pipeline.</param>
    /// <param name="logger">Logger para registrar errores internos.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta el middleware interceptando excepciones del pipeline.
    /// </summary>
    /// <param name="context">Contexto HTTP de la solicitud actual.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Mapea la excepción al código HTTP correspondiente y escribe la respuesta.
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorMessage) = exception switch
        {
            // ValidationException incluye detalle de campos con error
            Domain.Exceptions.ValidationException validationEx =>
                (HttpStatusCode.UnprocessableEntity, FormatValidationError(validationEx)),

            // UnauthorizedException → 401 (credenciales inválidas)
            UnauthorizedException unauthorizedEx =>
                (HttpStatusCode.Unauthorized, unauthorizedEx.Message),

            // NotFoundException → 404
            NotFoundException notFoundEx =>
                (HttpStatusCode.NotFound, notFoundEx.Message),

            // ConflictException → 409
            ConflictException conflictEx =>
                (HttpStatusCode.Conflict, conflictEx.Message),

            // ForbiddenException → 403
            ForbiddenException forbiddenEx =>
                (HttpStatusCode.Forbidden, forbiddenEx.Message),

            // DomainException (base) → 422 Unprocessable Entity
            DomainException domainEx =>
                (HttpStatusCode.UnprocessableEntity, domainEx.Message),

            // DbUpdateConcurrencyException → 409 (puja simultánea rechazada)
            DbUpdateConcurrencyException =>
                (HttpStatusCode.Conflict, "Conflicto de concurrencia. Intente nuevamente."),

            // Cualquier otra excepción → 500 + log interno, sin stack trace al cliente
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor.")
        };

        // Registrar excepciones no controladas (500) con detalle completo
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Excepción no controlada: {Message}", exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>(null, errorMessage, null);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Formatea los errores de validación incluyendo los campos con error.
    /// </summary>
    private static string FormatValidationError(Domain.Exceptions.ValidationException ex)
    {
        // Construir mensaje con detalle de campos: "Error de validación. Campo1: error1; Campo2: error2"
        var fieldErrors = ex.Errors
            .SelectMany(kvp => kvp.Value.Select(v => $"{kvp.Key}: {v}"))
            .ToList();

        return fieldErrors.Count > 0
            ? $"{ex.Message} {string.Join("; ", fieldErrors)}"
            : ex.Message;
    }
}
