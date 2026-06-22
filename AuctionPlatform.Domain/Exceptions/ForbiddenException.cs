namespace AuctionPlatform.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando el usuario no tiene permisos. Mapea a HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message) { }
}
