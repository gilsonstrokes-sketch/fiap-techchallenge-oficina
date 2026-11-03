using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Tests;

public sealed class PlacaTests
{
    [Theory]
    [InlineData("abc-1234", "ABC1234")]
    [InlineData("abc 1d23", "ABC1D23")]
    [InlineData("ABC1D23", "ABC1D23")]
    public void Criar_ComPlacaAntigaOuMercosul_Normaliza(string entrada, string esperada)
    {
        Assert.Equal(esperada, Placa.Criar(entrada).Valor);
    }

    [Theory]
    [InlineData("AB12345")]
    [InlineData("ABC-12D3")]
    [InlineData("ABC@1234")]
    public void Criar_ComFormatoInvalido_LancaExcecao(string entrada)
    {
        Assert.Throws<ArgumentException>(() => Placa.Criar(entrada));
    }
}
