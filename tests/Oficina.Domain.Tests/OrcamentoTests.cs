using Oficina.Domain.Atendimento;
using Oficina.Domain.Estoque;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Tests;

public sealed class OrcamentoTests
{
    [Fact]
    public void Criar_CalculaTotalAPartirDasLinhasArredondadas()
    {
        var pecaId = Guid.NewGuid();
        var servicoId = Guid.NewGuid();
        var orcamento = Orcamento.Criar(
            1,
            [
                ItemOrcamento.Criar(TipoItemOrcamento.Peca, pecaId, "Filtro", 1m, Dinheiro.Criar(10m)),
                ItemOrcamento.Criar(TipoItemOrcamento.Servico, servicoId, "Troca", 1m, Dinheiro.Criar(20m))
            ]);

        Assert.Equal(10m, orcamento.Itens[0].Total.Valor);
        Assert.Equal(30m, orcamento.Total.Valor);
        Assert.True(orcamento.EhValido);
    }

    [Fact]
    public void Orcamento_NaoPermiteVersaoInvalidaNemListaVazia()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Orcamento.Criar(0, [ItemValido()]));
        Assert.Throws<ArgumentException>(() => Orcamento.Criar(1, []));
        Assert.Throws<ArgumentNullException>(() => Orcamento.Criar(1, null!));
        Assert.Throws<ArgumentException>(() => Orcamento.Criar(1, new ItemOrcamento[] { null! }));
    }

    [Fact]
    public void ItemOrcamento_ValidaIdentidadeQuantidadeETipo()
    {
        Assert.Throws<ArgumentException>(() => ItemOrcamento.Criar(TipoItemOrcamento.Peca, Guid.Empty, "Filtro", 1m, Dinheiro.Criar(10m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemOrcamento.Criar(TipoItemOrcamento.Peca, Guid.NewGuid(), "Filtro", 0m, Dinheiro.Criar(10m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemOrcamento.Criar(TipoItemOrcamento.Servico, Guid.NewGuid(), "Mão de obra", 1.5m, Dinheiro.Criar(10m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemOrcamento.Criar(TipoItemOrcamento.Peca, Guid.NewGuid(), "Filtro", 0.5m, Dinheiro.Criar(10m)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemOrcamento.Criar((TipoItemOrcamento)99, Guid.NewGuid(), "Filtro", 1m, Dinheiro.Criar(10m)));
    }

    [Fact]
    public void Orcamento_ExpõeListaDeItensSomenteLeitura()
    {
        var orcamento = Orcamento.Criar(1, [ItemValido()]);

        Assert.IsAssignableFrom<IReadOnlyList<ItemOrcamento>>(orcamento.Itens);
        Assert.Throws<NotSupportedException>(() => ((IList<ItemOrcamento>)orcamento.Itens).Clear());
    }

    [Fact]
    public void ConfirmacaoDeReserva_PermiteVazioQuandoOrcamentoTemSomenteServicos()
    {
        var inicio = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
        var os = OrdemDeServico.Criar(Guid.NewGuid(), Guid.NewGuid(), inicio);
        os.IniciarDiagnostico(inicio.AddSeconds(1));
        var servico = ItemOrcamento.Criar(TipoItemOrcamento.Servico, Guid.NewGuid(), "Diagnóstico", 1m, Dinheiro.Criar(0m));
        var orcamento = os.DisponibilizarOrcamento([servico], inicio.AddSeconds(2));
        os.AprovarOrcamento(inicio.AddSeconds(3));
        var confirmacao = ConfirmacaoReservaEstoque.Criar(os.Id, orcamento.Versao, []);

        Assert.True(confirmacao.CorrespondeA(os.Id, orcamento));
    }

    [Fact]
    public void ConfirmacaoDeReserva_RecusaPecasDuplicadasEQuantidadesInvalidas()
    {
        var pecaId = Guid.NewGuid();
        var osId = Guid.NewGuid();
        var primeiraReserva = EstoqueDaPeca.Criar(pecaId, 1m).Reservar(osId, 1, 1m);
        var segundaReserva = EstoqueDaPeca.Criar(pecaId, 2m).Reservar(osId, 1, 2m);

        Assert.Throws<ArgumentException>(() => ConfirmacaoReservaEstoque.Criar(
            osId,
            1,
            [primeiraReserva, segundaReserva]));
        Assert.Throws<ArgumentOutOfRangeException>(() => EstoqueDaPeca.Criar(pecaId, 1m).Reservar(osId, 1, 0m));
    }

    [Fact]
    public void ConfirmacaoDeReserva_AgregaLinhasDaMesmaPecaAntesDeComparar()
    {
        var inicio = new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
        var pecaId = Guid.NewGuid();
        var os = OrdemDeServico.Criar(Guid.NewGuid(), Guid.NewGuid(), inicio);
        os.IniciarDiagnostico(inicio.AddMinutes(1));
        var orcamento = os.DisponibilizarOrcamento(
        [
            ItemOrcamento.Criar(TipoItemOrcamento.Peca, pecaId, "Filtro", 1m, Dinheiro.Criar(10m)),
            ItemOrcamento.Criar(TipoItemOrcamento.Peca, pecaId, "Filtro adicional", 2m, Dinheiro.Criar(10m))
        ], inicio.AddMinutes(2));
        os.AprovarOrcamento(inicio.AddMinutes(3));
        var reserva = EstoqueDaPeca.Criar(pecaId, 3m).Reservar(os.Id, orcamento.Versao, 3m);
        var confirmacao = ConfirmacaoReservaEstoque.Criar(os.Id, orcamento.Versao, [reserva]);

        Assert.True(confirmacao.CorrespondeA(os.Id, orcamento));
    }

    private static ItemOrcamento ItemValido() =>
        ItemOrcamento.Criar(TipoItemOrcamento.Servico, Guid.NewGuid(), "Troca de óleo", 1m, Dinheiro.Criar(50m));
}
