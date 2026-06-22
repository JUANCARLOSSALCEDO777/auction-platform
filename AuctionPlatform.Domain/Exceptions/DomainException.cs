namespace AuctionPlatform.Domain.Exceptions;

/// <summary>
/// Excepción base de dominio. Mapea a HTTP 422 Unprocessable Entity.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
