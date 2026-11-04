using Oficina.Domain.Estoque;

namespace Oficina.Application.Abstractions.Persistence;

public interface IEstoqueDaPecaRepository
{
    Task<EstoqueDaPeca?> ObterPorPecaIdAsync(Guid pecaId, CancellationToken cancellationToken = default);
    void Adicionar(EstoqueDaPeca estoque);
}
