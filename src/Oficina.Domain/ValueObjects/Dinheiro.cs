namespace Oficina.Domain.ValueObjects;

/// <summary>Valor monetário em reais, sem casas fracionárias de centavo.</summary>
public readonly record struct Dinheiro
{
    private Dinheiro(decimal valor) => Valor = valor;

    public decimal Valor { get; }

    public static Dinheiro Criar(decimal valor)
    {
        if (valor < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valor), "O valor não pode ser negativo.");
        }

        if (valor != decimal.Round(valor, 2, MidpointRounding.AwayFromZero))
        {
            throw new ArgumentOutOfRangeException(nameof(valor), "O valor deve ter no máximo duas casas decimais.");
        }

        return new Dinheiro(valor);
    }

    public static Dinheiro Zero => new(0m);

    public static Dinheiro operator +(Dinheiro esquerdo, Dinheiro direito) => Criar(esquerdo.Valor + direito.Valor);
}
