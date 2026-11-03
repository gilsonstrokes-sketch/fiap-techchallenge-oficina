using Oficina.Domain.Estoque;

namespace Oficina.Domain.Tests;

public sealed class EstoqueDaPecaTests
{
    [Fact]
    public void Reservar_EBaixar_AtualizaDisponivelESaldoFisico()
    {
        var estoque = EstoqueDaPeca.Criar(Guid.NewGuid(), 5m);
        var osId = Guid.NewGuid();

        estoque.Reservar(osId, 1, 3m);

        Assert.Equal(5m, estoque.QuantidadeFisica);
        Assert.Equal(3m, estoque.QuantidadeReservada);
        Assert.Equal(2m, estoque.QuantidadeDisponivel);

        estoque.Baixar(osId, 1);

        Assert.Equal(2m, estoque.QuantidadeFisica);
        Assert.Equal(0m, estoque.QuantidadeReservada);
        Assert.Equal(2m, estoque.QuantidadeDisponivel);
    }

    [Fact]
    public void EstoqueInsuficiente_NaoCriaReservaNemPermiteSaldoNegativo()
    {
        var estoque = EstoqueDaPeca.Criar(Guid.NewGuid(), 2m);

        Assert.Throws<InvalidOperationException>(() => estoque.Reservar(Guid.NewGuid(), 1, 3m));

        Assert.Equal(2m, estoque.QuantidadeFisica);
        Assert.Equal(0m, estoque.QuantidadeReservada);
        Assert.Equal(2m, estoque.QuantidadeDisponivel);
    }

    [Fact]
    public void ReservaAdicional_ApenasPermiteQuantidadeDisponivel()
    {
        var estoque = EstoqueDaPeca.Criar(Guid.NewGuid(), 5m);
        estoque.Reservar(Guid.NewGuid(), 1, 3m);

        Assert.Throws<InvalidOperationException>(() => estoque.Reservar(Guid.NewGuid(), 1, 3m));

        Assert.Equal(2m, estoque.QuantidadeDisponivel);
    }

    [Fact]
    public void Estoque_RecusaQuantidadeNegativaReservaDuplicadaEBaixaSemReserva()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EstoqueDaPeca.Criar(Guid.NewGuid(), -1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => EstoqueDaPeca.Criar(Guid.NewGuid(), 1.5m));

        var estoque = EstoqueDaPeca.Criar(Guid.NewGuid(), 4m);
        var osId = Guid.NewGuid();
        Assert.Throws<ArgumentOutOfRangeException>(() => estoque.Reservar(osId, 1, 0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => estoque.Reservar(osId, 1, 0.5m));
        estoque.Reservar(osId, 1, 1m);
        Assert.Throws<InvalidOperationException>(() => estoque.Reservar(osId, 1, 1m));
        estoque.Baixar(osId, 1);
        Assert.Throws<InvalidOperationException>(() => estoque.Baixar(osId, 1));
    }

    [Fact]
    public void ReservaEhSeparadaPorVersaoDoOrcamento()
    {
        var estoque = EstoqueDaPeca.Criar(Guid.NewGuid(), 5m);
        var osId = Guid.NewGuid();
        var primeira = estoque.Reservar(osId, 1, 2m);
        var segunda = estoque.Reservar(osId, 2, 2m);

        Assert.Equal(1, primeira.VersaoOrcamento);
        Assert.Equal(2, segunda.VersaoOrcamento);
        Assert.Equal(1m, estoque.QuantidadeDisponivel);

        estoque.Baixar(osId, 1);

        Assert.Equal(3m, estoque.QuantidadeFisica);
        Assert.Equal(2m, estoque.QuantidadeReservada);
        Assert.Equal(1m, estoque.QuantidadeDisponivel);
    }
}
