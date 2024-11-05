namespace Poc.CRM.EfExtensible.Web.Application;

/// <summary>
/// Shared domain error type. Commonly used by handler resulsts to indicate a failure.
/// </summary>
/// <param name="message">Messsage of the Error</param>
/// <param name="details">Further details of the error</param>
public record DomainError(string message, string? details = null);