using Oficina.Domain.Contratos;

namespace Oficina.Domain.Atendimento;

/// <summary>
/// Confirmação tipada produzida pela orquestração da aplicação depois de reservar todas as peças.
/// A OS valida identidade, versão e quantidades sem depender do agregado de estoque.
/// </summary>
/// <remarks>
/// Este objeto valida coerência dos recibos, mas não é prova criptográfica nem garante que a reserva
/// continue ativa no banco. Os recibos são emitidos pelo agregado de estoque e vinculados à OS,
/// versão, peça e quantidade. A aplicação deve construí-lo somente após todas as reservas terem
/// sucesso e consumi-lo dentro da mesma transação que inicia a OS e baixa as reservas. A instância
/// pode ser reutilizada; idempotência, concorrência e validade persistida são responsabilidades da
/// aplicação/infraestrutura. A fábrica pública permite composição pela Application, mas recibos
/// obtidos de agregados em memória não comprovam uma reserva persistida autoritativa.
/// </remarks>
public sealed class ConfirmacaoReservaEstoque
{
    private readonly IReadOnlyList<ReservaPecaConfirmada> _reservas;

    private ConfirmacaoReservaEstoque(Guid ordemDeServicoId, int versaoOrcamento, IReadOnlyList<ReservaPecaConfirmada> reservas)
    {
        OrdemDeServicoId = ordemDeServicoId;
        VersaoOrcamento = versaoOrcamento;
        _reservas = reservas;
    }

    public Guid OrdemDeServicoId { get; }

    public int VersaoOrcamento { get; }

    public IReadOnlyList<ReservaPecaConfirmada> Reservas => _reservas;

    public static ConfirmacaoReservaEstoque Criar(
        Guid ordemDeServicoId,
        int versaoOrcamento,
        IEnumerable<ReservaPecaConfirmada> reservas)
    {
        // O método é público para permitir composição pela Application. ReservaPecaConfirmada tem
        // construtor interno e só é emitida por EstoqueDaPeca no fluxo normal do domínio.
        if (ordemDeServicoId == Guid.Empty) throw new ArgumentException("A OS é obrigatória.", nameof(ordemDeServicoId));
        if (versaoOrcamento <= 0) throw new ArgumentOutOfRangeException(nameof(versaoOrcamento));
        ArgumentNullException.ThrowIfNull(reservas);

        var copia = reservas.ToArray();
        if (copia.Any(reserva => reserva is null)) throw new ArgumentException("A confirmação não pode conter reservas nulas.", nameof(reservas));
        if (copia.Select(reserva => reserva.PecaId).Distinct().Count() != copia.Length)
        {
            throw new ArgumentException("A confirmação deve conter no máximo uma quantidade agregada por peça.", nameof(reservas));
        }

        if (copia.Any(reserva => reserva.OrdemDeServicoId != ordemDeServicoId || reserva.VersaoOrcamento != versaoOrcamento))
        {
            throw new ArgumentException("As reservas devem corresponder à OS e à versão informadas.", nameof(reservas));
        }

        return new ConfirmacaoReservaEstoque(ordemDeServicoId, versaoOrcamento, Array.AsReadOnly(copia));
    }

    public bool CorrespondeA(Guid ordemDeServicoId, Orcamento orcamento)
    {
        ArgumentNullException.ThrowIfNull(orcamento);
        if (OrdemDeServicoId != ordemDeServicoId || VersaoOrcamento != orcamento.Versao)
        {
            return false;
        }

        var pecasEsperadas = orcamento.Itens
            .Where(item => item.Tipo == TipoItemOrcamento.Peca)
            .GroupBy(item => item.ItemCatalogoId)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(item => item.Quantidade));
        var pecasConfirmadas = _reservas.ToDictionary(reserva => reserva.PecaId, reserva => reserva.Quantidade);

        return pecasEsperadas.Count == pecasConfirmadas.Count
            && pecasEsperadas.All(par => pecasConfirmadas.TryGetValue(par.Key, out var quantidade) && quantidade == par.Value);
    }
}
