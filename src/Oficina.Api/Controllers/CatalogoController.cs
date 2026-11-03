using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application;
using Oficina.Application.Models;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize(Policy = "Administrador")]
[Route("api/servicos")]
public sealed class ServicosController(OficinaApplicationService oficina) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CatalogoDto>> Listar(CancellationToken ct) => oficina.ListarServicosAsync(ct);
    [HttpGet("{id:guid}")]
    public Task<CatalogoDto> Obter(Guid id, CancellationToken ct) => oficina.ObterServicoAsync(id, ct);
    [HttpPost]
    public async Task<ActionResult<CatalogoDto>> Criar([FromBody] CatalogoRequest request, CancellationToken ct)
    {
        var item = await oficina.CriarServicoAsync(request.Nome, request.Descricao, request.Preco, ct);
        return CreatedAtAction(nameof(Obter), new { id = item.Id }, item);
    }
    [HttpPut("{id:guid}")]
    public Task<CatalogoDto> Alterar(Guid id, [FromBody] CatalogoRequest request, CancellationToken ct) => oficina.AlterarServicoAsync(id, request.Nome, request.Descricao, request.Preco, ct);
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken ct) { await oficina.InativarServicoAsync(id, ct); return NoContent(); }
    [HttpPost("{id:guid}/reativacao")]
    public Task<CatalogoDto> Reativar(Guid id, CancellationToken ct) => oficina.ReativarServicoAsync(id, ct);
}

[ApiController]
[Authorize(Policy = "Administrador")]
[Route("api/pecas")]
public sealed class PecasController(OficinaApplicationService oficina) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CatalogoDto>> Listar(CancellationToken ct) => oficina.ListarPecasAsync(ct);
    [HttpGet("{id:guid}")]
    public Task<CatalogoDto> Obter(Guid id, CancellationToken ct) => oficina.ObterPecaAsync(id, ct);
    [HttpPost]
    public async Task<ActionResult<CatalogoDto>> Criar([FromBody] CatalogoRequest request, CancellationToken ct)
    {
        var item = await oficina.CriarPecaAsync(request.Nome, request.Descricao, request.Preco, ct);
        return CreatedAtAction(nameof(Obter), new { id = item.Id }, item);
    }
    [HttpPut("{id:guid}")]
    public Task<CatalogoDto> Alterar(Guid id, [FromBody] CatalogoRequest request, CancellationToken ct) => oficina.AlterarPecaAsync(id, request.Nome, request.Descricao, request.Preco, ct);
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken ct) { await oficina.InativarPecaAsync(id, ct); return NoContent(); }
    [HttpPost("{id:guid}/reativacao")]
    public Task<CatalogoDto> Reativar(Guid id, CancellationToken ct) => oficina.ReativarPecaAsync(id, ct);
}

public sealed record CatalogoRequest(string Nome, string Descricao, decimal Preco);
