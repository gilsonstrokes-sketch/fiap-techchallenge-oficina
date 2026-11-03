using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Tests;

public sealed class DocumentoFiscalTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    public void Criar_ComCpfValido_NormalizaDocumento(string entrada, string esperado)
    {
        var documento = DocumentoFiscal.Criar(entrada);

        Assert.Equal(TipoDocumentoFiscal.Cpf, documento.Tipo);
        Assert.Equal(esperado, documento.Valor);
    }

    [Theory]
    [InlineData("529.982.247-24")]
    [InlineData("111.111.111-11")]
    [InlineData("123.456.789-00")]
    public void Criar_ComCpfInvalido_LancaArgumentException(string entrada)
    {
        Assert.Throws<ArgumentException>(() => DocumentoFiscal.Criar(entrada));
    }

    [Fact]
    public void Criar_ComCnpjNumericoValido_NormalizaDocumento()
    {
        var documento = DocumentoFiscal.Criar("11.222.333/0001-81");

        Assert.Equal(TipoDocumentoFiscal.Cnpj, documento.Tipo);
        Assert.Equal("11222333000181", documento.Valor);
    }

    [Fact]
    public void Criar_ComCnpjAlfanumericoValido_AceitaNovoFormato()
    {
        var documento = DocumentoFiscal.Criar("00.000.000/E08G-12");

        Assert.Equal(TipoDocumentoFiscal.Cnpj, documento.Tipo);
        Assert.Equal("00000000E08G12", documento.Valor);
    }

    [Theory]
    [InlineData("11.222.333/0001-80")]
    [InlineData("00.000.000/E08G-13")]
    [InlineData("00.000.000/!08G-12")]
    public void Criar_ComCnpjInvalido_LancaArgumentException(string entrada)
    {
        Assert.Throws<ArgumentException>(() => DocumentoFiscal.Criar(entrada));
    }

    [Fact]
    public void Criar_ComValorNuloOuVazio_LancaArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() => DocumentoFiscal.Criar(null!));
        Assert.Throws<ArgumentException>(() => DocumentoFiscal.Criar("   "));
    }
}
