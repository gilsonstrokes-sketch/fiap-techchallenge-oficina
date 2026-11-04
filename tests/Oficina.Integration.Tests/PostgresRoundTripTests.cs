using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Oficina.Domain.Atendimento;
using Oficina.Domain.Cadastros;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Estoque;
using Oficina.Domain.ValueObjects;
using Oficina.Infrastructure;
using Oficina.Infrastructure.Persistence;
using Xunit;

namespace Oficina.Integration.Tests;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OFICINA_TEST_POSTGRES")))
        {
            Skip = "Configure OFICINA_TEST_POSTGRES com uma connection string para PostgreSQL descartável.";
        }
    }
}

public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    private readonly string _connectionString;

    public PostgresDatabaseFixture()
    {
        var connectionString = Environment.GetEnvironmentVariable("OFICINA_TEST_POSTGRES")
            ?? throw new InvalidOperationException("OFICINA_TEST_POSTGRES não foi configurada.");
        _connectionString = connectionString;
    }

    public OficinaDbContext CriarContexto(params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseNpgsql(_connectionString)
            .AddInterceptors(interceptors)
            .Options;
        return new OficinaDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await using var dbContext = CriarContexto();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[Collection(PostgresIntegrationCollection.Name)]
public sealed class PostgresRoundTripTests(PostgresDatabaseFixture fixture)
{
    [PostgresFact]
    public async Task Agregados_PersistemERecuperamOrcamentosHistoricoEReservas()
    {
        await using var dbContext = fixture.CriarContexto();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var documentoFiscal = await CriarDocumentoFiscalUnicoAsync(dbContext);
            var placa = await CriarPlacaUnicaAsync(dbContext);
            var cliente = Cliente.Criar("Cliente Round Trip", documentoFiscal);
            var veiculo = Veiculo.Criar(cliente.Id, placa, "Marca", "Modelo", 2020);
            var servico = Servico.Criar("Revisão", "Revisão preventiva", Dinheiro.Criar(80m));
            var peca = Peca.Criar("Filtro", "Filtro de óleo", Dinheiro.Criar(12.50m));
            var estoque = EstoqueDaPeca.Criar(peca.Id, 8m);
            var agora = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
            var ordem = OrdemDeServico.Criar(cliente.Id, veiculo.Id, agora);
            ordem.IniciarDiagnostico(agora.AddMinutes(1));
            var itemAntigo = ItemOrcamento.Criar(TipoItemOrcamento.Peca, peca.Id, "Filtro versão 1", 1m, Dinheiro.Criar(10m));
            ordem.DisponibilizarOrcamento([itemAntigo], agora.AddMinutes(2));
            ordem.RejeitarOrcamento(agora.AddMinutes(3), "Revisar peça");
            var itemAtual = ItemOrcamento.Criar(TipoItemOrcamento.Peca, peca.Id, "Filtro versão 2", 2m, Dinheiro.Criar(12.50m));
            var orcamentoAtual = ordem.DisponibilizarOrcamento([itemAtual], agora.AddMinutes(4));
            ordem.AprovarOrcamento(agora.AddMinutes(5));
            estoque.Reservar(ordem.Id, orcamentoAtual.Versao, 2m);

            dbContext.AddRange(cliente, veiculo, servico, peca, estoque, ordem);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();

            var ordemRecuperada = await new EfOrdemDeServicoRepository(dbContext).ObterPorIdAsync(ordem.Id);
            var estoqueRecuperado = await new EfEstoqueDaPecaRepository(dbContext).ObterPorPecaIdAsync(peca.Id);
            var clienteRecuperado = await dbContext.Clientes.SingleAsync(item => item.Id == cliente.Id);
            var veiculoRecuperado = await dbContext.Veiculos.SingleAsync(item => item.Id == veiculo.Id);
            var pecaRecuperada = await dbContext.Pecas.SingleAsync(item => item.Id == peca.Id);
            var servicoRecuperado = await dbContext.Servicos.SingleAsync(item => item.Id == servico.Id);

            Assert.NotNull(ordemRecuperada);
            Assert.Equal(StatusOrdemDeServico.AguardandoAprovacao, ordemRecuperada.Status);
            Assert.True(ordemRecuperada.OrcamentoAprovado);
            Assert.Equal(2, ordemRecuperada.Orcamentos.Count);
            Assert.Equal(2, ordemRecuperada.ObterOrcamentoAtual()!.Versao);
            Assert.Equal("Filtro versão 1", ordemRecuperada.Orcamentos.Single(item => item.Versao == 1).Itens.Single().Descricao);
            Assert.Equal("Filtro versão 2", ordemRecuperada.ObterOrcamentoAtual()!.Itens.Single().Descricao);
            Assert.Equal(5, ordemRecuperada.Historico.Count);
            Assert.Equal(2, ordemRecuperada.DecisoesOrcamento.Count);
            Assert.Equal(2m, ordemRecuperada.ObterOrcamentoAtual()!.Itens.Single().Quantidade);
            Assert.Equal(25m, ordemRecuperada.ObterOrcamentoAtual()!.Total.Valor);

            Assert.NotNull(estoqueRecuperado);
            Assert.Equal(8m, estoqueRecuperado.QuantidadeFisica);
            Assert.Equal(2m, estoqueRecuperado.QuantidadeReservada);
            Assert.Equal(ordem.Id, estoqueRecuperado.ObterReservaConfirmada(ordem.Id, 2)!.OrdemDeServicoId);
            Assert.Equal(cliente.DocumentoFiscal, clienteRecuperado.DocumentoFiscal);
            Assert.Equal(veiculo.Placa, veiculoRecuperado.Placa);
            Assert.Equal(peca.Preco, pecaRecuperada.Preco);
            Assert.Equal(servico.Preco, servicoRecuperado.Preco);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }

    private static async Task<DocumentoFiscal> CriarDocumentoFiscalUnicoAsync(OficinaDbContext dbContext)
    {
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

    private static async Task<Placa> CriarPlacaUnicaAsync(OficinaDbContext dbContext)
    {
        const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        while (true)
        {
            var placaTexto = string.Concat(
                letras[Random.Shared.Next(letras.Length)],
                letras[Random.Shared.Next(letras.Length)],
                letras[Random.Shared.Next(letras.Length)],
                Random.Shared.Next(10),
                letras[Random.Shared.Next(letras.Length)],
                Random.Shared.Next(10),
                Random.Shared.Next(10));
            var placa = Placa.Criar(placaTexto);
            if (!await dbContext.Veiculos.AnyAsync(veiculo => veiculo.Placa == placa))
                return placa;
        }
    }

    private static int CalcularDigitoCpf(string digitos, int pesoInicial)
    {
        var soma = digitos.Select((digito, indice) => (digito - '0') * (pesoInicial - indice)).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}

public sealed class OficinaEfModelTests
{
    [Fact]
    public void ModeloEf_ConstruiMapeamentosSemAbrirConexao()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseNpgsql("Host=localhost;Database=oficina_model_test;Username=test;Password=test")
            .Options;
        using var dbContext = new OficinaDbContext(options);

        var model = dbContext.GetService<IDesignTimeModel>().Model;

        Assert.Equal("ordens_de_servico", model.FindEntityType(typeof(OrdemDeServico))!.GetTableName());
        Assert.Equal("estoques_peca", model.FindEntityType(typeof(EstoqueDaPeca))!.GetTableName());
        Assert.Equal("reservas_estoque", model.FindEntityType(typeof(ReservaDeEstoque))!.GetTableName());
        Assert.Contains(model.FindEntityType(typeof(OrdemDeServico))!.GetProperties(), property => property.Name == "xmin" && property.IsConcurrencyToken);
        Assert.Contains(model.FindEntityType(typeof(EstoqueDaPeca))!.GetProperties(), property => property.Name == "xmin" && property.IsConcurrencyToken);
    }

    [Fact]
    public void ModeloEf_RestringePrecisaoDosValoresEAExclusaoDeEstoqueReservado()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseNpgsql("Host=localhost;Database=oficina_model_test;Username=test;Password=test")
            .Options;
        using var dbContext = new OficinaDbContext(options);
        var model = dbContext.GetService<IDesignTimeModel>().Model;

        var itemType = model.FindEntityType(typeof(ItemOrcamento))!;
        Assert.Equal("numeric", itemType.FindProperty(nameof(ItemOrcamento.Quantidade))!.GetColumnType());
        Assert.Equal("numeric", itemType.FindProperty(nameof(ItemOrcamento.ValorUnitario))!.GetColumnType());
        Assert.Contains(itemType.GetCheckConstraints(), constraint => constraint.Sql.Contains("quantidade = trunc(quantidade)", StringComparison.Ordinal));
        Assert.Contains(itemType.GetCheckConstraints(), constraint => constraint.Sql.Contains("trunc(valor_unitario, 2)", StringComparison.Ordinal));

        var reservaType = model.FindEntityType(typeof(ReservaDeEstoque))!;
        Assert.Contains(
            reservaType.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(EstoqueDaPeca)
                && foreignKey.DeleteBehavior == Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
    }
}
