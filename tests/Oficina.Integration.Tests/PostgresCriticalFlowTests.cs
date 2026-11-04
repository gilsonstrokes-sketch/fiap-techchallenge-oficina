using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Oficina.Application;
using Oficina.Application.Abstractions.Persistence;
using Oficina.Application.Models;
using Oficina.Domain.Atendimento;
using Oficina.Domain.Cadastros;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Estoque;
using Oficina.Domain.ValueObjects;
using Oficina.Infrastructure;
using Oficina.Infrastructure.Persistence;

namespace Oficina.Integration.Tests;

[CollectionDefinition("PostgreSQL integration")]
public sealed class PostgresIntegrationCollection : ICollectionFixture<PostgresDatabaseFixture>
{
    public const string Name = "PostgreSQL integration";
}

[Collection(PostgresIntegrationCollection.Name)]
public sealed class PostgresCriticalFlowTests(PostgresDatabaseFixture fixture)
{
    [PostgresFact]
    public async Task AprovarOrcamento_ReservaPecasEInicioBaixaEstoque()
    {
        var cenario = await CriarCenarioOrdemAsync(estoqueInicial: 5m);

        try
        {
            await using var dbContext = fixture.CriarContexto();
            var app = CriarAplicacao(dbContext);

            var aprovada = await app.AprovarOrcamentoAsync(cenario.OrdemId, CancellationToken.None);
            Assert.True(aprovada.OrcamentoAprovado);
            Assert.Equal("AguardandoAprovacao", aprovada.Status);

            dbContext.ChangeTracker.Clear();
            var estoqueReservado = await new EfEstoqueDaPecaRepository(dbContext).ObterPorPecaIdAsync(cenario.PecaId);
            Assert.NotNull(estoqueReservado);
            Assert.Equal(5m, estoqueReservado.QuantidadeFisica);
            Assert.Equal(2m, estoqueReservado.QuantidadeReservada);
            Assert.Equal(3m, estoqueReservado.QuantidadeDisponivel);

            var iniciada = await app.IniciarExecucaoAsync(cenario.OrdemId, CancellationToken.None);
            Assert.Equal("EmExecucao", iniciada.Status);

            dbContext.ChangeTracker.Clear();
            var estoqueBaixado = await new EfEstoqueDaPecaRepository(dbContext).ObterPorPecaIdAsync(cenario.PecaId);
            Assert.NotNull(estoqueBaixado);
            Assert.Equal(3m, estoqueBaixado.QuantidadeFisica);
            Assert.Equal(0m, estoqueBaixado.QuantidadeReservada);
            Assert.Equal(3m, estoqueBaixado.QuantidadeDisponivel);
        }
        finally
        {
            await LimparCenarioOrdemAsync(cenario);
        }
    }

    [PostgresFact]
    public async Task AprovarOrcamento_FalhaAposGravacaoReverteOrdemEReserva()
    {
        var cenario = await CriarCenarioOrdemAsync(estoqueInicial: 5m);

        try
        {
            var interceptor = new FalharAposGravacaoInterceptor();
            await using (var dbContext = fixture.CriarContexto(interceptor))
            {
                var app = CriarAplicacao(dbContext);
                await Assert.ThrowsAsync<PersistenciaSimuladaException>(
                    () => app.AprovarOrcamentoAsync(cenario.OrdemId, CancellationToken.None));
            }

            Assert.True(interceptor.GravacaoConcluida);
            await using var verificacao = fixture.CriarContexto();
            var ordem = await new EfOrdemDeServicoRepository(verificacao).ObterPorIdAsync(cenario.OrdemId);
            var estoque = await new EfEstoqueDaPecaRepository(verificacao).ObterPorPecaIdAsync(cenario.PecaId);

            Assert.NotNull(ordem);
            Assert.False(ordem.OrcamentoAprovado);
            Assert.Equal("AguardandoAprovacao", ordem.Status.ToString());
            Assert.NotNull(estoque);
            Assert.Equal(5m, estoque.QuantidadeFisica);
            Assert.Equal(0m, estoque.QuantidadeReservada);
        }
        finally
        {
            await LimparCenarioOrdemAsync(cenario);
        }
    }

    [PostgresFact]
    public async Task Estoque_ComDoisDbContextsEmEstadoObsoleto_RetornaConflitoEPerdeUmaAtualizacao()
    {
        var peca = Peca.Criar("Peça concorrência", "Peça para teste de concorrência", Dinheiro.Criar(1m));
        await using (var inicializacao = fixture.CriarContexto())
        {
            inicializacao.AddRange(peca, EstoqueDaPeca.Criar(peca.Id, 3m));
            await inicializacao.SaveChangesAsync();
        }

        try
        {
            await using var primeiroContexto = fixture.CriarContexto();
            await using var segundoContexto = fixture.CriarContexto();
            var primeiroEstoque = await new EfEstoqueDaPecaRepository(primeiroContexto).ObterPorPecaIdAsync(peca.Id);
            var segundoEstoque = await new EfEstoqueDaPecaRepository(segundoContexto).ObterPorPecaIdAsync(peca.Id);
            Assert.NotNull(primeiroEstoque);
            Assert.NotNull(segundoEstoque);

            primeiroEstoque.RegistrarEntrada(1m);
            segundoEstoque.RegistrarEntrada(1m);

            await new EfUnitOfWork(primeiroContexto).CommitAsync();
            var conflito = await Assert.ThrowsAsync<ConcurrencyConflictException>(
                () => new EfUnitOfWork(segundoContexto).CommitAsync());
            Assert.Contains("alterados por outra operação", conflito.Message, StringComparison.OrdinalIgnoreCase);

            await using var verificacao = fixture.CriarContexto();
            var estoquePersistido = await new EfEstoqueDaPecaRepository(verificacao).ObterPorPecaIdAsync(peca.Id);
            Assert.NotNull(estoquePersistido);
            Assert.Equal(4m, estoquePersistido.QuantidadeFisica);
        }
        finally
        {
            await using var limpeza = fixture.CriarContexto();
            await using var transacao = await limpeza.Database.BeginTransactionAsync();
            await limpeza.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM reservas_estoque WHERE peca_id = {peca.Id}");
            await limpeza.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM estoques_peca WHERE peca_id = {peca.Id}");
            await limpeza.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM pecas WHERE id = {peca.Id}");
            await transacao.CommitAsync();
        }
    }

    private async Task<CenarioOrdem> CriarCenarioOrdemAsync(decimal estoqueInicial)
    {
        var cliente = Cliente.Criar("Cliente teste integração", await CriarDocumentoFiscalUnicoAsync());
        var veiculo = Veiculo.Criar(cliente.Id, await CriarPlacaUnicaAsync(), "Marca", "Modelo", 2022);
        var peca = Peca.Criar("Peça teste integração", "Peça de cenário PostgreSQL", Dinheiro.Criar(10m));
        var estoque = EstoqueDaPeca.Criar(peca.Id, estoqueInicial);
        var ordemId = Guid.Empty;

        try
        {
            await using (var dbContext = fixture.CriarContexto())
            {
                dbContext.AddRange(cliente, veiculo, peca, estoque);
                await dbContext.SaveChangesAsync();

                var app = CriarAplicacao(dbContext);
                var ordem = await app.CriarOrdemAsync(cliente.Id, veiculo.Id, CancellationToken.None);
                ordemId = ordem.Id;
                await app.IniciarDiagnosticoAsync(ordemId, CancellationToken.None);
                await app.GerarOrcamentoAsync(
                    ordemId,
                    [new ItemOrcamentoInput(TipoItemOrcamento.Peca, peca.Id, 2m)],
                    CancellationToken.None);
            }

            return new CenarioOrdem(cliente.Id, veiculo.Id, peca.Id, ordemId);
        }
        catch
        {
            await LimparCenarioOrdemAsync(new CenarioOrdem(cliente.Id, veiculo.Id, peca.Id, ordemId));
            throw;
        }
    }

    private async Task<DocumentoFiscal> CriarDocumentoFiscalUnicoAsync()
    {
        await using var dbContext = fixture.CriarContexto();
        while (true)
        {
            string baseCpf;
            do
            {
                baseCpf = Random.Shared.NextInt64(0, 1_000_000_000).ToString("D9");
            } while (baseCpf.Distinct().Count() == 1);

            var primeiro = CalcularDigitoCpf(baseCpf, 10);
            var segundo = CalcularDigitoCpf(baseCpf + primeiro, 11);
            var documento = DocumentoFiscal.Criar(baseCpf + primeiro + segundo);
            if (!await dbContext.Clientes.AnyAsync(cliente => cliente.DocumentoFiscal == documento))
                return documento;
        }
    }

    private async Task<Placa> CriarPlacaUnicaAsync()
    {
        const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        await using var dbContext = fixture.CriarContexto();
        while (true)
        {
            var texto = string.Concat(
                letras[Random.Shared.Next(letras.Length)],
                letras[Random.Shared.Next(letras.Length)],
                letras[Random.Shared.Next(letras.Length)],
                Random.Shared.Next(10),
                letras[Random.Shared.Next(letras.Length)],
                Random.Shared.Next(10),
                Random.Shared.Next(10));
            var placa = Placa.Criar(texto);
            if (!await dbContext.Veiculos.AnyAsync(veiculo => veiculo.Placa == placa))
                return placa;
        }
    }

    private async Task LimparCenarioOrdemAsync(CenarioOrdem cenario)
    {
        await using var dbContext = fixture.CriarContexto();
        await using var transacao = await dbContext.Database.BeginTransactionAsync();
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM reservas_estoque WHERE peca_id = {cenario.PecaId} AND ordem_de_servico_id = {cenario.OrdemId}");
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM ordens_de_servico WHERE id = {cenario.OrdemId}");
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM estoques_peca WHERE peca_id = {cenario.PecaId}");
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM pecas WHERE id = {cenario.PecaId}");
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM veiculos WHERE id = {cenario.VeiculoId}");
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM clientes WHERE id = {cenario.ClienteId}");
        await transacao.CommitAsync();
    }

    private static OficinaApplicationService CriarAplicacao(OficinaDbContext dbContext) => new(
        new EfClienteRepository(dbContext),
        new EfVeiculoRepository(dbContext),
        new EfServicoRepository(dbContext),
        new EfPecaRepository(dbContext),
        new EfOrdemDeServicoRepository(dbContext),
        new EfEstoqueDaPecaRepository(dbContext),
        new EfUnitOfWork(dbContext),
        TimeProvider.System);

    private static int CalcularDigitoCpf(string digitos, int pesoInicial)
    {
        var soma = digitos.Select((digito, indice) => (digito - '0') * (pesoInicial - indice)).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private sealed record CenarioOrdem(Guid ClienteId, Guid VeiculoId, Guid PecaId, Guid OrdemId);

    private sealed class FalharAposGravacaoInterceptor : SaveChangesInterceptor
    {
        private int _falhaPendente = 1;

        public bool GravacaoConcluida { get; private set; }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            GravacaoConcluida = true;
            if (Interlocked.Exchange(ref _falhaPendente, 0) == 1)
                throw new PersistenciaSimuladaException();

            return ValueTask.FromResult(result);
        }
    }

    private sealed class PersistenciaSimuladaException() : Exception("Falha simulada após a gravação do SaveChanges.");
}
