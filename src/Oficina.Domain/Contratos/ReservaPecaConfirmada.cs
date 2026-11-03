namespace Oficina.Domain.Contratos;

/// <summary>Recibo emitido pelo agregado de estoque após reservar uma peça para uma OS e versão.</summary>
public sealed class ReservaPecaConfirmada
{
    internal ReservaPecaConfirmada(Guid ordemDeServicoId, int versaoOrcamento, Guid pecaId, decimal quantidade)
    {
        OrdemDeServicoId = ordemDeServicoId;
        VersaoOrcamento = versaoOrcamento;
        PecaId = pecaId;
        Quantidade = quantidade;
    }

    public Guid OrdemDeServicoId { get; }

    public int VersaoOrcamento { get; }

    public Guid PecaId { get; }

    public decimal Quantidade { get; }
}
