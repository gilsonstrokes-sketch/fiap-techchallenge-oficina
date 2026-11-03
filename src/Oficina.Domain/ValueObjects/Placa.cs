using System.Text;

namespace Oficina.Domain.ValueObjects;

public sealed record Placa
{
    private Placa(string valor) => Valor = valor;

    public string Valor { get; }

    public static Placa Criar(string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        var normalizada = Normalizar(valor);
        if (!FormatoAntigo(normalizada) && !FormatoMercosul(normalizada))
        {
            throw new ArgumentException("A placa informada não corresponde ao formato antigo ou Mercosul.", nameof(valor));
        }

        return new Placa(normalizada);
    }

    private static string Normalizar(string valor)
    {
        var resultado = new StringBuilder(valor.Length);
        foreach (var caractere in valor)
        {
            if (char.IsWhiteSpace(caractere) || caractere is '-' or '.')
            {
                continue;
            }

            if (!char.IsAsciiLetterOrDigit(caractere))
            {
                throw new ArgumentException("A placa contém caracteres inválidos.", nameof(valor));
            }

            resultado.Append(char.ToUpperInvariant(caractere));
        }

        return resultado.ToString();
    }

    private static bool FormatoAntigo(string placa) =>
        placa.Length == 7
        && Letras(placa, 0, 3)
        && Digitos(placa, 3, 4);

    private static bool FormatoMercosul(string placa) =>
        placa.Length == 7
        && Letras(placa, 0, 3)
        && char.IsAsciiDigit(placa[3])
        && char.IsAsciiLetter(placa[4])
        && Digitos(placa, 5, 2);

    private static bool Letras(string texto, int inicio, int quantidade)
    {
        for (var indice = inicio; indice < inicio + quantidade; indice++)
        {
            if (!char.IsAsciiLetter(texto[indice]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Digitos(string texto, int inicio, int quantidade)
    {
        for (var indice = inicio; indice < inicio + quantidade; indice++)
        {
            if (!char.IsAsciiDigit(texto[indice]))
            {
                return false;
            }
        }

        return true;
    }
}
