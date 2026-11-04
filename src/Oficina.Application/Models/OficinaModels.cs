using Oficina.Domain.Atendimento;
using Oficina.Domain.Cadastros;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Estoque;

namespace Oficina.Application.Models;

public sealed record ClienteDto(Guid Id, string Nome, string DocumentoFiscal, bool Ativo)
{
    public static ClienteDto De(Cliente item) => new(item.Id, item.Nome, item.DocumentoFiscal.Valor, item.Ativo);
}

public sealed record VeiculoDto(Guid Id, Guid ClienteId, string Placa, string Marca, string Modelo, int Ano, bool Ativo)
{
    public static VeiculoDto De(Veiculo item) => new(item.Id, item.ClienteId, item.Placa.Valor, item.Marca, item.Modelo, item.Ano, item.Ativo);
}

public sealed record CatalogoDto(Guid Id, string Nome, string Descricao, decimal Preco, bool Ativo)
{
    public static CatalogoDto De(Servico item) => new(item.Id, item.Nome, item.Descricao, item.Preco.Valor, item.Ativo);
    public static CatalogoDto De(Peca item) => new(item.Id, item.Nome, item.Descricao, item.Preco.Valor, item.Ativo);
}

public sealed record ItemOrcamentoDto(TipoItemOrcamento Tipo, Guid ItemCatalogoId, string Descricao, decimal Quantidade, decimal ValorUnitario, decimal Total);
public sealed record OrcamentoDto(int Versao, decimal Total, IReadOnlyList<ItemOrcamentoDto> Itens);
public sealed record HistoricoOrdemDto(string? De, string Para, string Acao, DateTimeOffset OcorridoEm);
public sealed record DecisaoOrcamentoDto(bool Aprovado, int VersaoOrcamento, DateTimeOffset OcorridoEm, string? Motivo);
public sealed record OrdemDeServicoDto(
    Guid Id,
    Guid ClienteId,
    Guid VeiculoId,
    string Status,
    bool OrcamentoAprovado,
    DateTimeOffset? ExecucaoIniciadaEm,
    DateTimeOffset? ExecucaoFinalizadaEm,
    OrcamentoDto? OrcamentoAtual,
    IReadOnlyList<HistoricoOrdemDto> Historico,
    IReadOnlyList<DecisaoOrcamentoDto> Decisoes)
{
    public static OrdemDeServicoDto De(OrdemDeServico ordem) => new(
        ordem.Id,
        ordem.ClienteId,
        ordem.VeiculoId,
        ordem.Status.ToString(),
        ordem.OrcamentoAprovado,
        ordem.ExecucaoIniciadaEm,
        ordem.ExecucaoFinalizadaEm,
        ordem.ObterOrcamentoAtual() is { } orcamento
            ? new OrcamentoDto(orcamento.Versao, orcamento.Total.Valor, orcamento.Itens.Select(item =>
                new ItemOrcamentoDto(item.Tipo, item.ItemCatalogoId, item.Descricao, item.Quantidade, item.ValorUnitario.Valor, item.Total.Valor)).ToArray())
            : null,
        ordem.Historico.OrderBy(item => item.OcorridoEm).Select(item => new HistoricoOrdemDto(item.De?.ToString(), item.Para.ToString(), item.Acao, item.OcorridoEm)).ToArray(),
        ordem.DecisoesOrcamento.OrderBy(item => item.OcorridoEm).Select(item => new DecisaoOrcamentoDto(item.Aprovado, item.VersaoOrcamento, item.OcorridoEm, item.Motivo)).ToArray());
}

public sealed record ConsultaClienteOrdemDto(Guid OrdemDeServicoId, string Status, decimal? TotalOrcamentoAtual, DateTimeOffset? ExecucaoIniciadaEm, DateTimeOffset? ExecucaoFinalizadaEm);
public sealed record EstoqueDto(Guid PecaId, decimal QuantidadeFisica, decimal QuantidadeReservada, decimal QuantidadeDisponivel);
public sealed record MediaExecucaoDto(int OrdensConsideradas, double MediaHoras);
public sealed record ItemOrcamentoInput(TipoItemOrcamento Tipo, Guid ItemCatalogoId, decimal Quantidade);
