namespace Oficina.Application.Abstractions.Persistence;

/// <summary>Indica que outro processo alterou dados concorrentes antes da gravação.</summary>
public sealed class ConcurrencyConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);
