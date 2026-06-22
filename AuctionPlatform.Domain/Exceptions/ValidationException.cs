namespace AuctionPlatform.Domain.Exceptions;

/// <summary>
/// Excepción de validación con lista de errores por campo. Mapea a HTTP 422 + detalle de campos.
/// </summary>
public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Error de validación.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : base("Error de validación.")
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }
}
