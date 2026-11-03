using Oficina.Domain.Catalogo;

namespace Oficina.Application.Abstractions.Persistence;

public interface IServicoRepository
{
    Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Adicionar(Servico servico);
}
