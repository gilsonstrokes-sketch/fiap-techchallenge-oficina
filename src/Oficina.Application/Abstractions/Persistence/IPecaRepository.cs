using Oficina.Domain.Catalogo;

namespace Oficina.Application.Abstractions.Persistence;

public interface IPecaRepository
{
    Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Adicionar(Peca peca);
}
