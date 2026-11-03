using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Tests;

public sealed class DinheiroTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(19.99)]
    public void Criar_ComValorValido_AceitaZeroEAtéDuasCasas(decimal valor)
    {
        Assert.Equal(valor, Dinheiro.Criar(valor).Valor);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.234)]
    public void Criar_ComValorForaDaPolitica_LancaExcecao(decimal valor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Dinheiro.Criar(valor));
    }

    [Fact]
    public void Somar_SomaValoresMonetarios()
    {
        var total = Dinheiro.Criar(12.50m) + Dinheiro.Criar(0.75m);

        Assert.Equal(13.25m, total.Valor);
    }
}
