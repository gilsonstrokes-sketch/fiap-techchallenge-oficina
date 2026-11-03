using Oficina.Domain.Contratos;

namespace Oficina.Domain.Estoque;

/// <summary>Raiz por peça para saldo físico e reservas; não integra o agregado de catálogo Peca.</summary>
public sealed class EstoqueDaPeca
{
    private readonly Dictionary<ChaveReserva, decimal> _reservasPorOrdem = [];

    private EstoqueDaPeca(Guid pecaId, decimal quantidadeFisica)
    {
        PecaId = pecaId;
        QuantidadeFisica = quantidadeFisica;
    }

    public Guid PecaId { get; }

    public decimal QuantidadeFisica { get; private set; }

    public decimal QuantidadeReservada => _reservasPorOrdem.Values.Sum();

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
        var chave = new ChaveReserva(ordemDeServicoId, versaoOrcamento);
        if (_reservasPorOrdem.ContainsKey(chave))
        {
            throw new InvalidOperationException("Já existe uma reserva para esta OS, versão e peça.");
        }

        if (quantidade > QuantidadeDisponivel)
        {
            throw new InvalidOperationException("A quantidade disponível é insuficiente para a reserva.");
        }

        _reservasPorOrdem.Add(chave, quantidade);
        return new ReservaPecaConfirmada(ordemDeServicoId, versaoOrcamento, PecaId, quantidade);
    }

    public void Baixar(Guid ordemDeServicoId, int versaoOrcamento)
    {
        if (ordemDeServicoId == Guid.Empty) throw new ArgumentException("A ordem de serviço é obrigatória.", nameof(ordemDeServicoId));
        if (versaoOrcamento <= 0) throw new ArgumentOutOfRangeException(nameof(versaoOrcamento));
        if (!_reservasPorOrdem.Remove(new ChaveReserva(ordemDeServicoId, versaoOrcamento), out var quantidadeReservada))
        {
            throw new InvalidOperationException("Não há reserva para esta OS e peça.");
        }

        QuantidadeFisica -= quantidadeReservada;
    }

    private static void ExigirQuantidadePositiva(decimal quantidade)
    {
        if (quantidade <= 0 || quantidade != decimal.Truncate(quantidade))
        {
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade reservada deve ser um número inteiro positivo.");
        }
    }

    private readonly record struct ChaveReserva(Guid OrdemDeServicoId, int VersaoOrcamento);
}
