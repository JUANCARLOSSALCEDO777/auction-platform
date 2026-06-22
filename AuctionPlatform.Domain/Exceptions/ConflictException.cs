namespace AuctionPlatform.Domain.Exceptions;

/// <summary>
/// Excepción lanzada ante un conflicto de concurrencia o datos duplicados. Mapea a HTTP 409 Conflict.
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}
