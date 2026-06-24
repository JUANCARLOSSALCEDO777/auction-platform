namespace AuctionPlatform.Domain.Exceptions;

/// <summary>
/// Excepción que indica credenciales inválidas o falta de autenticación.
/// Mapea a HTTP 401 Unauthorized.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException) { }
}
