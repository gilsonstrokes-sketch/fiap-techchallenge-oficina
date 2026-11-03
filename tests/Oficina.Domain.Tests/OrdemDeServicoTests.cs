using Oficina.Domain.Atendimento;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Tests;

public sealed class OrdemDeServicoTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid PecaId = Guid.NewGuid();

    [Fact]
    public void FluxoDaOs_AprovacaoEInicioSaoAcoesSeparadas_ERejeicaoRetornaAoDiagnostico()
    {
        var os = CriarOs();
        os.IniciarDiagnostico(Inicio.AddMinutes(1));
        var primeiraVersao = os.DisponibilizarOrcamento(ItensDoOrcamento(15m), Inicio.AddMinutes(2));
        os.RejeitarOrcamento(Inicio.AddMinutes(3), "  Requer autorização adicional  ");

        Assert.Equal(StatusOrdemDeServico.EmDiagnostico, os.Status);
        Assert.Single(os.DecisoesOrcamento);
        Assert.False(os.DecisoesOrcamento.Single().Aprovado);
        Assert.Equal(1, os.DecisoesOrcamento.Single().VersaoOrcamento);
        Assert.Equal("Requer autorização adicional", os.DecisoesOrcamento.Single().Motivo);
        Assert.Equal(Inicio.AddMinutes(3), os.DecisoesOrcamento.Single().OcorridoEm);

        var segundaVersao = os.DisponibilizarOrcamento(ItensDoOrcamento(20m), Inicio.AddMinutes(4));
        os.AprovarOrcamento(Inicio.AddMinutes(5));

        Assert.Equal(StatusOrdemDeServico.AguardandoAprovacao, os.Status);
        Assert.True(os.OrcamentoAprovado);
        Assert.Equal(1, primeiraVersao.Versao);
        Assert.Equal(2, segundaVersao.Versao);
        Assert.Equal(130m, primeiraVersao.Total.Valor);
        Assert.Equal(140m, segundaVersao.Total.Valor);
        Assert.Equal(2, os.DecisoesOrcamento.Last().VersaoOrcamento);

        var confirmacao = CriarConfirmacao(os.Id, 2, 2m);
        os.IniciarExecucao(Inicio.AddMinutes(6), confirmacao);
        os.FinalizarExecucao(Inicio.AddMinutes(7));
        os.EntregarVeiculo(Inicio.AddMinutes(8));

        Assert.Equal(StatusOrdemDeServico.Entregue, os.Status);
        Assert.Equal(Inicio.AddMinutes(6), os.ExecucaoIniciadaEm);
        Assert.Equal(Inicio.AddMinutes(7), os.ExecucaoFinalizadaEm);
        Assert.Equal(8, os.Historico.Count);
    }

    [Fact]
    public void InicioSemEstoqueBloqueiaExecucaoEPreservaAprovacao()
    {
        var os = CriarOsAprovada();

        var semPecas = ConfirmacaoReservaEstoque.Criar(os.Id, 1, []);
        Assert.Throws<InvalidOperationException>(() => os.IniciarExecucao(Inicio.AddMinutes(3), semPecas));

        Assert.Equal(StatusOrdemDeServico.AguardandoAprovacao, os.Status);
        Assert.True(os.OrcamentoAprovado);
        Assert.Null(os.ExecucaoIniciadaEm);
    }

    [Fact]
    public void AcaoForaDoEstadoEsperado_NaoMudaEstado()
    {
        var os = CriarOs();

        Assert.Throws<InvalidOperationException>(() => os.FinalizarExecucao(Inicio.AddMinutes(1)));

        Assert.Equal(StatusOrdemDeServico.Recebida, os.Status);
        Assert.Single(os.Historico);
    }

    [Fact]
    public void AcaoComDataAnteriorAoHistorico_NaoMutaAgregado()
    {
        var os = CriarOs();
        os.IniciarDiagnostico(Inicio.AddMinutes(1));

        Assert.Throws<ArgumentOutOfRangeException>(() => os.DisponibilizarOrcamento(ItensDoOrcamento(15m), Inicio));

        Assert.Equal(StatusOrdemDeServico.EmDiagnostico, os.Status);
        Assert.Equal(2, os.Historico.Count);
    }

    [Fact]
    public void InicioDeExecucaoSemAprovacao_EReprovacaoDepoisDeAprovado_SaoBloqueados()
    {
        var os = CriarOs();
        os.IniciarDiagnostico(Inicio.AddMinutes(1));
        os.DisponibilizarOrcamento(ItensDoOrcamento(15m), Inicio.AddMinutes(2));

        var confirmacao = CriarConfirmacao(os.Id, 1, 2m);
        Assert.Throws<InvalidOperationException>(() => os.IniciarExecucao(Inicio.AddMinutes(3), confirmacao));
        os.AprovarOrcamento(Inicio.AddMinutes(3));
        Assert.Throws<InvalidOperationException>(() => os.AprovarOrcamento(Inicio.AddMinutes(4)));
        Assert.Throws<InvalidOperationException>(() => os.RejeitarOrcamento(Inicio.AddMinutes(4)));

        Assert.True(os.OrcamentoAprovado);
        Assert.Equal(StatusOrdemDeServico.AguardandoAprovacao, os.Status);
    }

    [Fact]
    public void NaoDisponibilizaOrcamentoVazioNemAprovaSemOrcamento()
    {
        var os = CriarOs();
        os.IniciarDiagnostico(Inicio.AddMinutes(1));

        Assert.Throws<ArgumentException>(() => os.DisponibilizarOrcamento([], Inicio.AddMinutes(2)));
        Assert.Equal(StatusOrdemDeServico.EmDiagnostico, os.Status);
        Assert.Empty(os.Orcamentos);
        Assert.Throws<InvalidOperationException>(() => os.AprovarOrcamento(Inicio.AddMinutes(2)));
    }

    [Fact]
    public void Rejeicao_AceitaMotivoOpcional()
    {
        var os = CriarOs();
        os.IniciarDiagnostico(Inicio.AddMinutes(1));
        var orcamento = os.DisponibilizarOrcamento(ItensDoOrcamento(15m), Inicio.AddMinutes(2));
        os.RejeitarOrcamento(Inicio.AddMinutes(3));

        var decisao = Assert.Single(os.DecisoesOrcamento);
        Assert.False(decisao.Aprovado);
        Assert.Equal(orcamento.Versao, decisao.VersaoOrcamento);
        Assert.Equal(Inicio.AddMinutes(3), decisao.OcorridoEm);
        Assert.Null(decisao.Motivo);
    }

    [Fact]
    public void ConfirmacaoDeReserva_ExigeOsVersaoPecasEQuantidadesExatas()
    {
        var os = CriarOsAprovada();

        Assert.Throws<InvalidOperationException>(() => os.IniciarExecucao(Inicio.AddMinutes(4), CriarConfirmacao(Guid.NewGuid(), 1, 2m)));
        Assert.Throws<InvalidOperationException>(() => os.IniciarExecucao(Inicio.AddMinutes(4), CriarConfirmacao(os.Id, 2, 2m)));
        Assert.Throws<InvalidOperationException>(() => os.IniciarExecucao(Inicio.AddMinutes(4), CriarConfirmacao(os.Id, 1, 1m)));

        Assert.Equal(StatusOrdemDeServico.AguardandoAprovacao, os.Status);
        Assert.True(os.OrcamentoAprovado);
    }

    [Fact]
    public void InicioDaExecucaoNaoPodeAntecederAprovacao()
    {
        var os = CriarOsAprovada();
        var confirmacao = CriarConfirmacao(os.Id, 1, 2m);

        Assert.Throws<ArgumentOutOfRangeException>(() => os.IniciarExecucao(Inicio.AddMinutes(2), confirmacao));

        Assert.Equal(StatusOrdemDeServico.AguardandoAprovacao, os.Status);
        Assert.Null(os.ExecucaoIniciadaEm);
    }

    [Fact]
    public void StatusOrdemDeServico_ContemSeisEstadosDoRequisito()
    {
        Assert.Equal(
            new[]
            {
                StatusOrdemDeServico.Recebida,
                StatusOrdemDeServico.EmDiagnostico,
                StatusOrdemDeServico.AguardandoAprovacao,
                StatusOrdemDeServico.EmExecucao,
                StatusOrdemDeServico.Finalizada,
                StatusOrdemDeServico.Entregue
            },
            Enum.GetValues<StatusOrdemDeServico>());
    }

    private static OrdemDeServico CriarOs() => OrdemDeServico.Criar(Guid.NewGuid(), Guid.NewGuid(), Inicio);

    private static OrdemDeServico CriarOsAprovada()
    {
        var os = CriarOs();
        os.IniciarDiagnostico(Inicio.AddMinutes(1));
        os.DisponibilizarOrcamento(ItensDoOrcamento(15m), Inicio.AddMinutes(2));
        os.AprovarOrcamento(Inicio.AddMinutes(3));
        return os;
    }

    private static ItemOrcamento[] ItensDoOrcamento(decimal precoPeca) =>
    [
        ItemOrcamento.Criar(TipoItemOrcamento.Servico, Guid.NewGuid(), "Revisão", 1m, Dinheiro.Criar(100m)),
        ItemOrcamento.Criar(TipoItemOrcamento.Peca, PecaId, "Filtro", 2m, Dinheiro.Criar(precoPeca))
    ];

    private static ConfirmacaoReservaEstoque CriarConfirmacao(Guid osId, int versao, decimal quantidade) =>
        ConfirmacaoReservaEstoque.Criar(osId, versao, [
            Oficina.Domain.Estoque.EstoqueDaPeca.Criar(PecaId, quantidade).Reservar(osId, versao, quantidade)
        ]);
}
