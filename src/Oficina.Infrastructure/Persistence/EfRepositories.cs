using Microsoft.EntityFrameworkCore;
using Oficina.Application.Abstractions.Persistence;
using Oficina.Domain.Atendimento;
using Oficina.Domain.Cadastros;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Estoque;
using Oficina.Domain.ValueObjects;

namespace Oficina.Infrastructure.Persistence;

public sealed class EfClienteRepository(OficinaDbContext dbContext) : IClienteRepository
{
    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Clientes.SingleOrDefaultAsync(cliente => cliente.Id == id, cancellationToken);

    public Task<Cliente?> ObterPorDocumentoAsync(DocumentoFiscal documento, CancellationToken cancellationToken = default) =>
        dbContext.Clientes.SingleOrDefaultAsync(cliente => cliente.DocumentoFiscal == documento, cancellationToken);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Clientes.OrderBy(cliente => cliente.Nome).ToListAsync(cancellationToken);

    public void Adicionar(Cliente cliente) => dbContext.Clientes.Add(cliente);
}

public sealed class EfVeiculoRepository(OficinaDbContext dbContext) : IVeiculoRepository
{
    public Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Veiculos.SingleOrDefaultAsync(veiculo => veiculo.Id == id, cancellationToken);

    public Task<Veiculo?> ObterPorPlacaAsync(Placa placa, CancellationToken cancellationToken = default) =>
        dbContext.Veiculos.SingleOrDefaultAsync(veiculo => veiculo.Placa == placa, cancellationToken);

    public async Task<IReadOnlyList<Veiculo>> ListarAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Veiculos.OrderBy(veiculo => veiculo.Placa).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default) =>
        await dbContext.Veiculos.Where(veiculo => veiculo.ClienteId == clienteId).OrderBy(veiculo => veiculo.Placa).ToListAsync(cancellationToken);

    public void Adicionar(Veiculo veiculo) => dbContext.Veiculos.Add(veiculo);
}

public sealed class EfServicoRepository(OficinaDbContext dbContext) : IServicoRepository
{
    public Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Servicos.SingleOrDefaultAsync(servico => servico.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Servico>> ListarAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Servicos.OrderBy(servico => servico.Nome).ToListAsync(cancellationToken);

    public void Adicionar(Servico servico) => dbContext.Servicos.Add(servico);
}

public sealed class EfPecaRepository(OficinaDbContext dbContext) : IPecaRepository
{
    public Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Pecas.SingleOrDefaultAsync(peca => peca.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Peca>> ListarAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Pecas.OrderBy(peca => peca.Nome).ToListAsync(cancellationToken);

    public void Adicionar(Peca peca) => dbContext.Pecas.Add(peca);
}

public sealed class EfOrdemDeServicoRepository(OficinaDbContext dbContext) : IOrdemDeServicoRepository
{
    public Task<OrdemDeServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.OrdensDeServico
            .AsSplitQuery()
            .Include(ordem => ordem.Orcamentos).ThenInclude(orcamento => orcamento.Itens)
            .Include(ordem => ordem.Historico)
            .Include(ordem => ordem.DecisoesOrcamento)
            .SingleOrDefaultAsync(ordem => ordem.Id == id, cancellationToken);

    public async Task<IReadOnlyList<OrdemDeServico>> ListarAsync(CancellationToken cancellationToken = default) =>
        await dbContext.OrdensDeServico
            .AsSplitQuery()
            .Include(ordem => ordem.Orcamentos).ThenInclude(orcamento => orcamento.Itens)
            .Include(ordem => ordem.Historico)
            .Include(ordem => ordem.DecisoesOrcamento)
            .OrderByDescending(ordem => ordem.ExecucaoIniciadaEm)
            .ToListAsync(cancellationToken);

    public void Adicionar(OrdemDeServico ordemDeServico) => dbContext.OrdensDeServico.Add(ordemDeServico);
}

public sealed class EfEstoqueDaPecaRepository(OficinaDbContext dbContext) : IEstoqueDaPecaRepository
{
    public Task<EstoqueDaPeca?> ObterPorPecaIdAsync(Guid pecaId, CancellationToken cancellationToken = default) =>
        dbContext.EstoquesDePeca.Include(estoque => estoque.Reservas)
            .SingleOrDefaultAsync(estoque => estoque.PecaId == pecaId, cancellationToken);

    public void Adicionar(EstoqueDaPeca estoque) => dbContext.EstoquesDePeca.Add(estoque);
}

public sealed class EfUnitOfWork(OficinaDbContext dbContext) : IUnitOfWork
{
    public async Task ExecutarEmTransacaoAsync(Func<CancellationToken, Task> operacao, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operacao);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await operacao(cancellationToken);
        await PersistirAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await PersistirAsync(cancellationToken);
    }

    private async Task PersistirAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(
                "Os dados foram alterados por outra operação. Recarregue o estado e tente novamente.",
                exception);
        }
    }
}
