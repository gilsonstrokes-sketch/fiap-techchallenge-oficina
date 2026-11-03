namespace Oficina.Domain.Atendimento;

/// <summary>Raiz do agregado que controla o ciclo de vida e as ações da OS.</summary>
public sealed class OrdemDeServico
{
    private readonly List<TransicaoOrdemDeServico> _historico = [];
    private readonly List<DecisaoOrcamento> _decisoesOrcamento = [];
    private readonly List<Orcamento> _orcamentos = [];

    // EF Core usa este construtor privado e preenche as propriedades com setters privados.
    private OrdemDeServico() { }

    public Guid Id { get; private set; }

    public Guid ClienteId { get; private set; }

    public Guid VeiculoId { get; private set; }

    public StatusOrdemDeServico Status { get; private set; }

    public bool OrcamentoAprovado { get; private set; }

    public DateTimeOffset? ExecucaoIniciadaEm { get; private set; }

    public DateTimeOffset? ExecucaoFinalizadaEm { get; private set; }

    public IReadOnlyCollection<TransicaoOrdemDeServico> Historico => _historico.AsReadOnly();

    public IReadOnlyCollection<DecisaoOrcamento> DecisoesOrcamento => _decisoesOrcamento.AsReadOnly();

    public IReadOnlyCollection<Orcamento> Orcamentos => _orcamentos.AsReadOnly();

    public Orcamento? ObterOrcamentoAtual() => _orcamentos.MaxBy(orcamento => orcamento.Versao);

    /// <summary>Abre uma OS para as referências de cliente e veículo já validadas pela camada Application.</summary>
    /// <remarks>Application deve confirmar que o veículo pertence ao cliente antes de chamar este método.</remarks>
    public static OrdemDeServico Criar(Guid clienteId, Guid veiculoId, DateTimeOffset recebidaEm)
    {
        if (clienteId == Guid.Empty) throw new ArgumentException("O cliente é obrigatório.", nameof(clienteId));
        if (veiculoId == Guid.Empty) throw new ArgumentException("O veículo é obrigatório.", nameof(veiculoId));

        var ordem = new OrdemDeServico
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            Status = StatusOrdemDeServico.Recebida
        };
        ordem._historico.Add(new TransicaoOrdemDeServico(null, ordem.Status, "Receber", recebidaEm));
        return ordem;
    }

    public void IniciarDiagnostico(DateTimeOffset ocorridoEm) =>
        Transicionar(StatusOrdemDeServico.Recebida, StatusOrdemDeServico.EmDiagnostico, "IniciarDiagnostico", ocorridoEm);

    public Orcamento DisponibilizarOrcamento(IEnumerable<ItemOrcamento> itens, DateTimeOffset ocorridoEm)
    {
        ExigirStatus(StatusOrdemDeServico.EmDiagnostico);
        ExigirDataNaoAnterior(ocorridoEm);

        var versao = _orcamentos.Count + 1;
        var orcamento = Orcamento.Criar(versao, itens);
        _orcamentos.Add(orcamento);
        Transicionar(StatusOrdemDeServico.EmDiagnostico, StatusOrdemDeServico.AguardandoAprovacao, "DisponibilizarOrcamento", ocorridoEm);
        OrcamentoAprovado = false;
        return orcamento;
    }

    public void AprovarOrcamento(DateTimeOffset ocorridoEm)
    {
        ExigirStatus(StatusOrdemDeServico.AguardandoAprovacao);
        ExigirDataNaoAnterior(ocorridoEm);
        if (OrcamentoAprovado) throw new InvalidOperationException("O orçamento já foi aprovado.");
        var orcamento = ExigirOrcamentoAtual();

        OrcamentoAprovado = true;
        _decisoesOrcamento.Add(DecisaoOrcamento.Aprovar(orcamento.Versao, ocorridoEm));
    }

    public void RejeitarOrcamento(DateTimeOffset ocorridoEm, string? motivo = null)
    {
        ExigirStatus(StatusOrdemDeServico.AguardandoAprovacao);
        ExigirDataNaoAnterior(ocorridoEm);
        if (OrcamentoAprovado) throw new InvalidOperationException("O orçamento já foi aprovado.");
        var orcamento = ExigirOrcamentoAtual();

        _decisoesOrcamento.Add(DecisaoOrcamento.Rejeitar(orcamento.Versao, ocorridoEm, motivo));
        OrcamentoAprovado = false;
        Transicionar(StatusOrdemDeServico.AguardandoAprovacao, StatusOrdemDeServico.EmDiagnostico, "RejeitarOrcamento", ocorridoEm);
    }

    public void IniciarExecucao(DateTimeOffset ocorridoEm, ConfirmacaoReservaEstoque confirmacaoReserva)
    {
        ExigirStatus(StatusOrdemDeServico.AguardandoAprovacao);
        if (!OrcamentoAprovado) throw new InvalidOperationException("O orçamento precisa estar aprovado para iniciar a execução.");
        ArgumentNullException.ThrowIfNull(confirmacaoReserva);
        var orcamento = ExigirOrcamentoAtual();
        if (!confirmacaoReserva.CorrespondeA(Id, orcamento))
        {
            throw new InvalidOperationException("A confirmação não corresponde às peças, quantidades, OS e versão aprovadas.");
        }

        Transicionar(StatusOrdemDeServico.AguardandoAprovacao, StatusOrdemDeServico.EmExecucao, "IniciarExecucao", ocorridoEm);
        ExecucaoIniciadaEm = ocorridoEm;
    }

    public void FinalizarExecucao(DateTimeOffset ocorridoEm)
    {
        ExigirStatus(StatusOrdemDeServico.EmExecucao);
        Transicionar(StatusOrdemDeServico.EmExecucao, StatusOrdemDeServico.Finalizada, "FinalizarExecucao", ocorridoEm);
        ExecucaoFinalizadaEm = ocorridoEm;
    }

    public void EntregarVeiculo(DateTimeOffset ocorridoEm) =>
        Transicionar(StatusOrdemDeServico.Finalizada, StatusOrdemDeServico.Entregue, "EntregarVeiculo", ocorridoEm);

    private void Transicionar(StatusOrdemDeServico esperado, StatusOrdemDeServico novo, string acao, DateTimeOffset ocorridoEm)
    {
        ExigirStatus(esperado);
        ExigirDataNaoAnterior(ocorridoEm);

        Status = novo;
        _historico.Add(new TransicaoOrdemDeServico(esperado, novo, acao, ocorridoEm));
    }

    private void ExigirStatus(StatusOrdemDeServico esperado)
    {
        if (Status != esperado)
        {
            throw new InvalidOperationException($"A ação não é permitida quando a OS está no estado {Status}.");
        }
    }

    private Orcamento ExigirOrcamentoAtual()
    {
        var orcamento = ObterOrcamentoAtual();
        if (orcamento is null || !orcamento.EhValido)
        {
            throw new InvalidOperationException("A OS precisa ter um orçamento válido e não vazio.");
        }

        return orcamento;
    }

    private void ExigirDataNaoAnterior(DateTimeOffset ocorridoEm)
    {
        var ultimaData = _historico[^1].OcorridoEm;
        if (_decisoesOrcamento.Count > 0 && _decisoesOrcamento[^1].OcorridoEm > ultimaData)
        {
            ultimaData = _decisoesOrcamento[^1].OcorridoEm;
        }

        if (ocorridoEm < ultimaData)
        {
            throw new ArgumentOutOfRangeException(nameof(ocorridoEm), "A data da ação não pode anteceder o último evento da OS.");
        }
    }
}

public sealed record TransicaoOrdemDeServico(
    StatusOrdemDeServico? De,
    StatusOrdemDeServico Para,
    string Acao,
    DateTimeOffset OcorridoEm);

public sealed class DecisaoOrcamento
{
    private DecisaoOrcamento(bool aprovado, int versaoOrcamento, DateTimeOffset ocorridoEm, string? motivo)
    {
        Aprovado = aprovado;
        VersaoOrcamento = versaoOrcamento;
        OcorridoEm = ocorridoEm;
        Motivo = motivo;
    }

    public bool Aprovado { get; }

    public int VersaoOrcamento { get; }

    public DateTimeOffset OcorridoEm { get; }

    public string? Motivo { get; }

    internal static DecisaoOrcamento Aprovar(int versao, DateTimeOffset ocorridoEm) =>
        new(true, versao, ocorridoEm, null);

    internal static DecisaoOrcamento Rejeitar(int versao, DateTimeOffset ocorridoEm, string? motivo) =>
        new(false, versao, ocorridoEm, string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim());
}
