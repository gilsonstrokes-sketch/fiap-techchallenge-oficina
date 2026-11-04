using Oficina.Domain.Cadastros;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.Abstractions.Persistence;

public interface IVeiculoRepository
{
    Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Veiculo?> ObterPorPlacaAsync(Placa placa, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Veiculo>> ListarAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    void Adicionar(Veiculo veiculo);
}
