using Oficina.Domain.Atendimento;

namespace Oficina.Application.Abstractions.Persistence;

public interface IOrdemDeServicoRepository
{
    Task<OrdemDeServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrdemDeServico>> ListarAsync(CancellationToken cancellationToken = default);
    void Adicionar(OrdemDeServico ordemDeServico);
}
