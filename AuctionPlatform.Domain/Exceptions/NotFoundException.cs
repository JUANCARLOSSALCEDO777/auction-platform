namespace AuctionPlatform.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando un recurso no se encuentra. Mapea a HTTP 404 Not Found.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string entityName, object id)
        : base($"{entityName} con id '{id}' no fue encontrado.") { }
}
