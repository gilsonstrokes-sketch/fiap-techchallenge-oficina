using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Atendimento;

/// <summary>Versão imutável do orçamento com snapshots das linhas e total calculado.</summary>
public sealed class Orcamento
{
    private readonly IReadOnlyList<ItemOrcamento> _itens;

    private Orcamento(int versao, IReadOnlyList<ItemOrcamento> itens, Dinheiro total)
    {
        Versao = versao;
        _itens = itens;
        Total = total;
    }

    public int Versao { get; }

    public IReadOnlyList<ItemOrcamento> Itens => _itens;

    public Dinheiro Total { get; }

    public bool EhValido => _itens.Count > 0;

    public static Orcamento Criar(int versao, IEnumerable<ItemOrcamento> itens)
    {
        if (versao <= 0) throw new ArgumentOutOfRangeException(nameof(versao), "A versão deve ser positiva.");
        ArgumentNullException.ThrowIfNull(itens);

        var copia = itens.ToArray();
        if (copia.Length == 0) throw new ArgumentException("O orçamento deve conter ao menos um serviço ou peça.", nameof(itens));
        if (copia.Any(item => item is null)) throw new ArgumentException("O orçamento não pode conter itens nulos.", nameof(itens));

        var total = copia.Aggregate(Dinheiro.Zero, (acumulado, item) => acumulado + item.Total);
        return new Orcamento(versao, Array.AsReadOnly(copia), total);
    }
}
