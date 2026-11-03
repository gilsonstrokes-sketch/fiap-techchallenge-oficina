using System.Text;

namespace Oficina.Domain.ValueObjects;

public sealed record DocumentoFiscal
{
    private static readonly int[] PesosCnpjPrimeiroDigito = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] PesosCnpjSegundoDigito = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    private DocumentoFiscal(string valor, TipoDocumentoFiscal tipo)
    {
        Valor = valor;
        Tipo = tipo;
    }

    public string Valor { get; }

    public TipoDocumentoFiscal Tipo { get; }

    public static DocumentoFiscal Criar(string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);

        var normalizado = Normalizar(valor);
        if (normalizado.Length == 11 && EhCpfValido(normalizado))
        {
            return new DocumentoFiscal(normalizado, TipoDocumentoFiscal.Cpf);
        }

        if (normalizado.Length == 14 && EhCnpjValido(normalizado))
        {
            return new DocumentoFiscal(normalizado, TipoDocumentoFiscal.Cnpj);
        }

        throw new ArgumentException("O documento fiscal informado é inválido.", nameof(valor));
    }

    private static string Normalizar(string valor)
    {
        var resultado = new StringBuilder(valor.Length);

        foreach (var caractere in valor)
        {
            if (char.IsWhiteSpace(caractere) || caractere is '.' or '/' or '-')
            {
                continue;
            }

            if (!char.IsAsciiLetterOrDigit(caractere))
            {
                throw new ArgumentException("O documento fiscal contém caracteres inválidos.", nameof(valor));
            }

            resultado.Append(char.ToUpperInvariant(caractere));
        }

        return resultado.ToString();
    }

    private static bool EhCpfValido(string cpf)
    {
        if (!cpf.All(char.IsAsciiDigit) || cpf.All(caractere => caractere == cpf[0]))
        {
            return false;
        }

        var primeiroDigito = CalcularDigitoCpf(cpf.AsSpan(0, 9), 10);
        var segundoDigito = CalcularDigitoCpf(cpf.AsSpan(0, 10), 11);

        return cpf[9] - '0' == primeiroDigito && cpf[10] - '0' == segundoDigito;
    }

    private static int CalcularDigitoCpf(ReadOnlySpan<char> digitos, int pesoInicial)
    {
        var soma = 0;
        for (var indice = 0; indice < digitos.Length; indice++)
        {
            soma += (digitos[indice] - '0') * (pesoInicial - indice);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static bool EhCnpjValido(string cnpj)
    {
        for (var indice = 0; indice < 12; indice++)
        {
            if (!char.IsAsciiLetterOrDigit(cnpj[indice]))
            {
                return false;
            }
        }

        if (!char.IsAsciiDigit(cnpj[12]) || !char.IsAsciiDigit(cnpj[13]))
        {
            return false;
        }

        if (cnpj.All(char.IsAsciiDigit) && cnpj.All(caractere => caractere == cnpj[0]))
        {
            return false;
        }

        var primeiroDigito = CalcularDigitoCnpj(cnpj.AsSpan(0, 12), PesosCnpjPrimeiroDigito);
        var segundoDigito = CalcularDigitoCnpj(cnpj.AsSpan(0, 13), PesosCnpjSegundoDigito);

        return cnpj[12] - '0' == primeiroDigito && cnpj[13] - '0' == segundoDigito;
    }

    private static int CalcularDigitoCnpj(ReadOnlySpan<char> baseCnpj, IReadOnlyList<int> pesos)
    {
        var soma = 0;
        for (var indice = 0; indice < baseCnpj.Length; indice++)
        {
            // A regra oficial converte cada caractere pelo valor ASCII menos 48.
            soma += (baseCnpj[indice] - 48) * pesos[indice];
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
