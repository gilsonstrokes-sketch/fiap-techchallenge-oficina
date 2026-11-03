namespace Oficina.Application.Abstractions.Persistence;

/// <summary>Persiste atomicamente as alterações rastreadas por uma operação de aplicação.</summary>
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task ExecutarEmTransacaoAsync(Func<CancellationToken, Task> operacao, CancellationToken cancellationToken = default);
}
