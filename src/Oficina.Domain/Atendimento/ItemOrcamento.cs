using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Atendimento;

/// <summary>Snapshot imutável de um serviço ou peça incluído em uma versão do orçamento.</summary>
public sealed class ItemOrcamento
{
    private ItemOrcamento(
        TipoItemOrcamento tipo,
        Guid itemCatalogoId,
        string descricao,
        decimal quantidade,
        Dinheiro valorUnitario,
        Dinheiro total)
    {
        Tipo = tipo;
        ItemCatalogoId = itemCatalogoId;
        Descricao = descricao;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        Total = total;
    }

    public TipoItemOrcamento Tipo { get; }

    public Guid ItemCatalogoId { get; }

    public string Descricao { get; }

    public decimal Quantidade { get; }

    public Dinheiro ValorUnitario { get; }

    public Dinheiro Total { get; }

    public static ItemOrcamento Criar(
        TipoItemOrcamento tipo,
        Guid itemCatalogoId,
        string descricao,
        decimal quantidade,
        Dinheiro valorUnitario)
    {
        if (!Enum.IsDefined(tipo)) throw new ArgumentOutOfRangeException(nameof(tipo));
        if (itemCatalogoId == Guid.Empty) throw new ArgumentException("O item de catálogo é obrigatório.", nameof(itemCatalogoId));
        ArgumentException.ThrowIfNullOrWhiteSpace(descricao);
        if (quantidade <= 0 || quantidade != decimal.Truncate(quantidade))
        {
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade deve ser um número inteiro positivo.");
        }

        var totalDaLinha = decimal.Round(
            valorUnitario.Valor * quantidade,
            2,
            MidpointRounding.AwayFromZero);

        return new ItemOrcamento(tipo, itemCatalogoId, descricao.Trim(), quantidade, valorUnitario, Dinheiro.Criar(totalDaLinha));
    }
}
