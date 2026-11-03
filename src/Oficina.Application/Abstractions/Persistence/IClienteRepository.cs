using Oficina.Domain.Cadastros;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.Abstractions.Persistence;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorDocumentoAsync(DocumentoFiscal documento, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken = default);
    void Adicionar(Cliente cliente);
}
