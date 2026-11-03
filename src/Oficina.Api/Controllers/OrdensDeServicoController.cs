using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.Security;
using Oficina.Application;
using Oficina.Application.Models;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize(Policy = "Administrador")]
[Route("api/ordens-de-servico")]
public sealed class OrdensDeServicoController(OficinaApplicationService oficina) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<OrdemDeServicoDto>> Listar(CancellationToken ct) => oficina.ListarOrdensAsync(ct);
    [HttpGet("{id:guid}")]
    public Task<OrdemDeServicoDto> Obter(Guid id, CancellationToken ct) => oficina.ObterOrdemAsync(id, ct);
    [HttpPost]
    public async Task<ActionResult<OrdemDeServicoDto>> Criar([FromBody] CriarOrdemRequest request, CancellationToken ct)
    {
        var ordem = await oficina.CriarOrdemPorDocumentoAsync(request.DocumentoFiscal, request.VeiculoId, ct);
        return CreatedAtAction(nameof(Obter), new { id = ordem.Id }, ordem);
    }
    [HttpPost("{id:guid}/diagnostico")]
    public Task<OrdemDeServicoDto> IniciarDiagnostico(Guid id, CancellationToken ct) => oficina.IniciarDiagnosticoAsync(id, ct);
    [HttpPost("{id:guid}/orcamentos")]
    public Task<OrdemDeServicoDto> GerarOrcamento(Guid id, [FromBody] OrcamentoRequest request, CancellationToken ct) => oficina.GerarOrcamentoAsync(id, request.Itens, ct);
    [HttpPost("{id:guid}/orcamentos/aprovacao")]
    public Task<OrdemDeServicoDto> AprovarOrcamento(Guid id, CancellationToken ct) => oficina.AprovarOrcamentoAsync(id, ct);
    [HttpPost("{id:guid}/orcamentos/rejeicao")]
    public Task<OrdemDeServicoDto> RejeitarOrcamento(Guid id, [FromBody] RejeitarOrcamentoRequest request, CancellationToken ct) => oficina.RejeitarOrcamentoAsync(id, request.Motivo, ct);
    [HttpPost("{id:guid}/orcamentos/substituicao")]
    public Task<OrdemDeServicoDto> SubstituirOrcamento(Guid id, [FromBody] OrcamentoRequest request, CancellationToken ct) => oficina.SubstituirOrcamentoAprovadoAsync(id, request.Itens, ct);
    [HttpPost("{id:guid}/execucao/inicio")]
    public Task<OrdemDeServicoDto> IniciarExecucao(Guid id, CancellationToken ct) => oficina.IniciarExecucaoAsync(id, ct);
    [HttpPost("{id:guid}/execucao/finalizacao")]
    public Task<OrdemDeServicoDto> FinalizarExecucao(Guid id, CancellationToken ct) => oficina.FinalizarExecucaoAsync(id, ct);
    [HttpPost("{id:guid}/entrega")]
    public Task<OrdemDeServicoDto> Entregar(Guid id, CancellationToken ct) => oficina.EntregarVeiculoAsync(id, ct);
}

[ApiController]
[Authorize(Policy = "ConsultaDaOrdem")]
[Route("api/consulta/ordens-de-servico")]
public sealed class ConsultaClienteController(OficinaApplicationService oficina) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConsultaClienteOrdemDto>> Consultar(Guid id, CancellationToken ct)
    {
        var claim = User.FindFirst("ordem_id")?.Value;
        if (!Guid.TryParse(claim, out var ordemToken) || ordemToken != id) return Forbid();
        return Ok(await oficina.ObterConsultaClienteAsync(id, ct));
    }
}

[ApiController]
[Authorize(Policy = "Administrador")]
[Route("api/indicadores")]
public sealed class IndicadoresController(OficinaApplicationService oficina) : ControllerBase
{
    [HttpGet("tempo-medio-execucao")]
    public Task<MediaExecucaoDto> MediaExecucao(CancellationToken ct) => oficina.ObterMediaExecucaoAsync(ct);
}

public sealed record CriarOrdemRequest(string DocumentoFiscal, Guid VeiculoId);
public sealed record OrcamentoRequest(IReadOnlyCollection<ItemOrcamentoInput> Itens);
public sealed record RejeitarOrcamentoRequest(string? Motivo);
