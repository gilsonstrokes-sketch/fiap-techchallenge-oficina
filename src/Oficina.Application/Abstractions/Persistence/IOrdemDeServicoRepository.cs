using Oficina.Domain.Atendimento;

namespace Oficina.Application.Abstractions.Persistence;

public interface IOrdemDeServicoRepository
{
    Task<OrdemDeServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Adicionar(OrdemDeServico ordemDeServico);
}
