using Oficina.Application;
using Oficina.Application.Abstractions.Persistence;
using Oficina.Application.Models;
using Oficina.Domain.Atendimento;
using Oficina.Domain.Cadastros;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Estoque;
using Oficina.Domain.ValueObjects;

namespace Oficina.Integration.Tests;

/// <summary>Application unit tests use repository fakes; they do not stand in for PostgreSQL integration tests.</summary>
public sealed class ApplicationWorkflowUnitTests
{
    [Fact]
    public async Task AprovarEIniciarExecucao_ReservaEBaixaPecasPersistidas()
    {
        var store = new FakeStore();
        var app = store.CriarAplicacao();
        var ordem = await CriarOrdemComPeca(app, 3);
        var pecaId = ordem.PecaId;

        var aprovada = await app.AprovarOrcamentoAsync(ordem.OrdemId, default);
        Assert.True(aprovada.OrcamentoAprovado);
        Assert.Equal(2, store.Estoques[pecaId].QuantidadeReservada);
        Assert.Equal(3, store.Estoques[pecaId].QuantidadeFisica);

        var iniciada = await app.IniciarExecucaoAsync(ordem.OrdemId, default);
        Assert.Equal("EmExecucao", iniciada.Status);
        Assert.Equal(1, store.Estoques[pecaId].QuantidadeFisica);
        Assert.Equal(0, store.Estoques[pecaId].QuantidadeReservada);
    }

    [Fact]
    public async Task AprovarSemEstoque_PreservaAprovacaoEBloqueiaInicio()
    {
        var store = new FakeStore();
        var app = store.CriarAplicacao();
        var ordem = await CriarOrdemComPeca(app, 0);

        var aprovada = await app.AprovarOrcamentoAsync(ordem.OrdemId, default);
        Assert.True(aprovada.OrcamentoAprovado);
        Assert.Equal("AguardandoAprovacao", aprovada.Status);

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(() => app.IniciarExecucaoAsync(ordem.OrdemId, default));
        Assert.Contains("Estoque", erro.Message, StringComparison.OrdinalIgnoreCase);
        var persistida = await app.ObterOrdemAsync(ordem.OrdemId, default);
        Assert.True(persistida.OrcamentoAprovado);
        Assert.Equal("AguardandoAprovacao", persistida.Status);
    }

    [Fact]
    public async Task CriarOrdem_RejeitaVeiculoVinculadoAOutroCliente()
    {
        var store = new FakeStore();
        var app = store.CriarAplicacao();
        var clienteA = await app.CriarClienteAsync("Cliente A", "529.982.247-25", default);
        var clienteB = await app.CriarClienteAsync("Cliente B", "111.444.777-35", default);
        var veiculo = await app.CriarVeiculoAsync(clienteA.Id, "ABC1234", "Fiat", "Uno", 2010, default);

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(() => app.CriarOrdemAsync(clienteB.Id, veiculo.Id, default));
        Assert.Contains("não pertence", erro.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<(Guid OrdemId, Guid PecaId)> CriarOrdemComPeca(OficinaApplicationService app, decimal estoqueInicial)
    {
        var cliente = await app.CriarClienteAsync("Cliente Teste", "529.982.247-25", default);
        var veiculo = await app.CriarVeiculoAsync(cliente.Id, "ABC1234", "Fiat", "Uno", 2010, default);
        var peca = await app.CriarPecaAsync("Filtro", "Filtro de óleo", 10m, default);
        if (estoqueInicial > 0)
            await app.RegistrarEntradaEstoqueAsync(peca.Id, estoqueInicial, default);
        var ordem = await app.CriarOrdemAsync(cliente.Id, veiculo.Id, default);
        await app.IniciarDiagnosticoAsync(ordem.Id, default);
        await app.GerarOrcamentoAsync(ordem.Id, [new ItemOrcamentoInput(TipoItemOrcamento.Peca, peca.Id, 2)], default);
        return (ordem.Id, peca.Id);
    }

    private sealed class FakeStore : IClienteRepository, IVeiculoRepository, IServicoRepository,
        IPecaRepository, IOrdemDeServicoRepository, IEstoqueDaPecaRepository, IUnitOfWork
    {
        private readonly Dictionary<Guid, Cliente> _clientes = [];
        private readonly Dictionary<Guid, Veiculo> _veiculos = [];
        private readonly Dictionary<Guid, Servico> _servicos = [];
        private readonly Dictionary<Guid, Peca> _pecas = [];
        private readonly Dictionary<Guid, OrdemDeServico> _ordens = [];
        public Dictionary<Guid, EstoqueDaPeca> Estoques { get; } = [];

        public OficinaApplicationService CriarAplicacao() => new(this, this, this, this, this, this, this, TimeProvider.System);
        Task<Cliente?> IClienteRepository.ObterPorIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_clientes.GetValueOrDefault(id));
        Task<Cliente?> IClienteRepository.ObterPorDocumentoAsync(DocumentoFiscal documento, CancellationToken cancellationToken) => Task.FromResult(_clientes.Values.SingleOrDefault(x => x.DocumentoFiscal == documento));
        Task<IReadOnlyList<Cliente>> IClienteRepository.ListarAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Cliente>>(_clientes.Values.ToArray());
        void IClienteRepository.Adicionar(Cliente cliente) => _clientes.Add(cliente.Id, cliente);
        Task<Veiculo?> IVeiculoRepository.ObterPorIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_veiculos.GetValueOrDefault(id));
        Task<Veiculo?> IVeiculoRepository.ObterPorPlacaAsync(Placa placa, CancellationToken cancellationToken) => Task.FromResult(_veiculos.Values.SingleOrDefault(x => x.Placa == placa));
        Task<IReadOnlyList<Veiculo>> IVeiculoRepository.ListarAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Veiculo>>(_veiculos.Values.ToArray());
        Task<IReadOnlyList<Veiculo>> IVeiculoRepository.ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Veiculo>>(_veiculos.Values.Where(x => x.ClienteId == clienteId).ToArray());
        void IVeiculoRepository.Adicionar(Veiculo veiculo) => _veiculos.Add(veiculo.Id, veiculo);
        Task<Servico?> IServicoRepository.ObterPorIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_servicos.GetValueOrDefault(id));
        Task<IReadOnlyList<Servico>> IServicoRepository.ListarAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Servico>>(_servicos.Values.ToArray());
        void IServicoRepository.Adicionar(Servico servico) => _servicos.Add(servico.Id, servico);
        Task<Peca?> IPecaRepository.ObterPorIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_pecas.GetValueOrDefault(id));
        Task<IReadOnlyList<Peca>> IPecaRepository.ListarAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Peca>>(_pecas.Values.ToArray());
        void IPecaRepository.Adicionar(Peca peca) => _pecas.Add(peca.Id, peca);
        Task<OrdemDeServico?> IOrdemDeServicoRepository.ObterPorIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_ordens.GetValueOrDefault(id));
        Task<IReadOnlyList<OrdemDeServico>> IOrdemDeServicoRepository.ListarAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<OrdemDeServico>>(_ordens.Values.ToArray());
        void IOrdemDeServicoRepository.Adicionar(OrdemDeServico ordemDeServico) => _ordens.Add(ordemDeServico.Id, ordemDeServico);
        public Task<EstoqueDaPeca?> ObterPorPecaIdAsync(Guid pecaId, CancellationToken cancellationToken = default) => Task.FromResult(Estoques.GetValueOrDefault(pecaId));
        void IEstoqueDaPecaRepository.Adicionar(EstoqueDaPeca estoque) => Estoques.Add(estoque.PecaId, estoque);
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public async Task ExecutarEmTransacaoAsync(Func<CancellationToken, Task> operacao, CancellationToken cancellationToken = default) => await operacao(cancellationToken);
    }
}
