using Oficina.Domain.Catalogo;
using Oficina.Domain.Cadastros;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Tests;

public sealed class CadastrosDomainTests
{
    [Fact]
    public void CriarCliente_ComNomeEDocumentoValidos_NormalizaNome()
    {
        var cliente = Cliente.Criar("  Ana Souza  ", DocumentoFiscal.Criar("529.982.247-25"));

        Assert.Equal("Ana Souza", cliente.Nome);
        Assert.Equal("52998224725", cliente.DocumentoFiscal.Valor);
        Assert.NotEqual(Guid.Empty, cliente.Id);

        cliente.AlterarNome("  Ana Silva ");
        Assert.Equal("Ana Silva", cliente.Nome);
    }

    [Fact]
    public void CriarVeiculo_VinculaClienteEArmazenaDadosObrigatorios()
    {
        var clienteId = Guid.NewGuid();
        var veiculo = Veiculo.Criar(clienteId, Placa.Criar("abc-1234"), "  Fiat ", " Uno ", 2010);

        Assert.Equal(clienteId, veiculo.ClienteId);
        Assert.Equal("ABC1234", veiculo.Placa.Valor);
        Assert.Equal("Fiat", veiculo.Marca);
        Assert.Equal("Uno", veiculo.Modelo);
        Assert.Equal(2010, veiculo.Ano);

        veiculo.AlterarDados(Placa.Criar("XYZ-9876"), "VW", "Gol", 2018);
        Assert.Equal("XYZ9876", veiculo.Placa.Valor);
        Assert.Equal("VW", veiculo.Marca);
        Assert.Equal("Gol", veiculo.Modelo);
        Assert.Equal(2018, veiculo.Ano);
    }

    [Fact]
    public void CriarServicoEPeca_AceitaPrecoZeroEValoresEmCentavos()
    {
        var servico = Servico.Criar("Alinhamento", "Alinhamento de direção", Dinheiro.Criar(0m));
        var peca = Peca.Criar("Filtro", "Filtro de óleo", Dinheiro.Criar(24.95m));

        Assert.Equal(0m, servico.Preco.Valor);
        Assert.Equal(24.95m, peca.Preco.Valor);

        servico.Alterar("Balanceamento", "Balanceamento de rodas", Dinheiro.Criar(15m));
        peca.Alterar("Pastilha", "Pastilha de freio", Dinheiro.Criar(39.90m));
        Assert.Equal(15m, servico.Preco.Valor);
        Assert.Equal(39.90m, peca.Preco.Valor);
    }
}
