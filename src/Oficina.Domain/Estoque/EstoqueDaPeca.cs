using Oficina.Domain.Contratos;

namespace Oficina.Domain.Estoque;

/// <summary>Raiz por peça para saldo físico e reservas; não integra o agregado de catálogo Peca.</summary>
public sealed class EstoqueDaPeca
{
    private readonly List<ReservaDeEstoque> _reservas = [];

    private EstoqueDaPeca() { }

    private EstoqueDaPeca(Guid pecaId, decimal quantidadeFisica)
    {
        PecaId = pecaId;
        QuantidadeFisica = quantidadeFisica;
    }

    public Guid PecaId { get; private set; }

    public decimal QuantidadeFisica { get; private set; }

    public decimal QuantidadeReservada => _reservas.Sum(reserva => reserva.Quantidade);

    /// <summary>Reservas persistíveis que pertencem a este agregado.</summary>
    public IReadOnlyCollection<ReservaDeEstoque> Reservas => _reservas.AsReadOnly();

    public decimal QuantidadeDisponivel => QuantidadeFisica - QuantidadeReservada;

    public static EstoqueDaPeca Criar(Guid pecaId, decimal quantidadeFisica)
    {
        if (pecaId == Guid.Empty) throw new ArgumentException("A peça é obrigatória.", nameof(pecaId));
        if (quantidadeFisica < 0 || quantidadeFisica != decimal.Truncate(quantidadeFisica))
        {
            throw new ArgumentOutOfRangeException(nameof(quantidadeFisica), "A quantidade física deve ser um número inteiro não negativo.");
        }
        return new EstoqueDaPeca(pecaId, quantidadeFisica);
    }

    /// <summary>Reserva uma quantidade e emite um recibo tipado para a OS e versão do orçamento.</summary>
    /// <remarks>A aplicação só deve chamar após aprovação e compor a confirmação após todas as reservas terem sucesso.</remarks>
    public ReservaPecaConfirmada Reservar(Guid ordemDeServicoId, int versaoOrcamento, decimal quantidade)
    {
        if (ordemDeServicoId == Guid.Empty) throw new ArgumentException("A ordem de serviço é obrigatória.", nameof(ordemDeServicoId));
        if (versaoOrcamento <= 0) throw new ArgumentOutOfRangeException(nameof(versaoOrcamento));
        ExigirQuantidadePositiva(quantidade);
        if (_reservas.Any(reserva => reserva.CorrespondeA(ordemDeServicoId, versaoOrcamento)))
        {
            throw new InvalidOperationException("Já existe uma reserva para esta OS, versão e peça.");
        }

        if (quantidade > QuantidadeDisponivel)
        {
            throw new InvalidOperationException("A quantidade disponível é insuficiente para a reserva.");
        }

        _reservas.Add(new ReservaDeEstoque(ordemDeServicoId, versaoOrcamento, quantidade));
        return new ReservaPecaConfirmada(ordemDeServicoId, versaoOrcamento, PecaId, quantidade);
    }

    /// <summary>Emite um recibo somente se a reserva ainda existir no estado carregado do agregado.</summary>
    public ReservaPecaConfirmada? ObterReservaConfirmada(Guid ordemDeServicoId, int versaoOrcamento)
    {
        var reserva = _reservas.SingleOrDefault(item => item.CorrespondeA(ordemDeServicoId, versaoOrcamento));
        return reserva is null
            ? null
            : new ReservaPecaConfirmada(ordemDeServicoId, versaoOrcamento, PecaId, reserva.Quantidade);
    }

    public void Baixar(Guid ordemDeServicoId, int versaoOrcamento)
    {
        if (ordemDeServicoId == Guid.Empty) throw new ArgumentException("A ordem de serviço é obrigatória.", nameof(ordemDeServicoId));
        if (versaoOrcamento <= 0) throw new ArgumentOutOfRangeException(nameof(versaoOrcamento));
        var reserva = _reservas.SingleOrDefault(item => item.CorrespondeA(ordemDeServicoId, versaoOrcamento));
        if (reserva is null)
        {
            throw new InvalidOperationException("Não há reserva para esta OS e peça.");
        }

        _reservas.Remove(reserva);
        QuantidadeFisica -= reserva.Quantidade;
    }

    private static void ExigirQuantidadePositiva(decimal quantidade)
    {
        if (quantidade <= 0 || quantidade != decimal.Truncate(quantidade))
        {
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade reservada deve ser um número inteiro positivo.");
        }
    }
}

/// <summary>Reserva individual que integra o agregado de estoque e pode ser persistida em tabela própria.</summary>
public sealed class ReservaDeEstoque
{
    private ReservaDeEstoque() { }

    internal ReservaDeEstoque(Guid ordemDeServicoId, int versaoOrcamento, decimal quantidade)
    {
        OrdemDeServicoId = ordemDeServicoId;
        VersaoOrcamento = versaoOrcamento;
        Quantidade = quantidade;
    }

    public Guid OrdemDeServicoId { get; private set; }

    public int VersaoOrcamento { get; private set; }

    public decimal Quantidade { get; private set; }

    internal bool CorrespondeA(Guid ordemDeServicoId, int versaoOrcamento) =>
        OrdemDeServicoId == ordemDeServicoId && VersaoOrcamento == versaoOrcamento;
}
