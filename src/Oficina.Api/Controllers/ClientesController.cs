using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application;
using Oficina.Application.Models;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize(Policy = "Administrador")]
[Route("api/clientes")]
public sealed class ClientesController(OficinaApplicationService oficina) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ClienteDto>> Listar(CancellationToken ct) => oficina.ListarClientesAsync(ct);

    [HttpGet("{id:guid}")]
    public Task<ClienteDto> Obter(Guid id, CancellationToken ct) => oficina.ObterClienteAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Criar([FromBody] CriarClienteRequest request, CancellationToken ct)
    {
        var cliente = await oficina.CriarClienteAsync(request.Nome, request.DocumentoFiscal, ct);
        return CreatedAtAction(nameof(Obter), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id:guid}")]
    public Task<ClienteDto> Alterar(Guid id, [FromBody] AlterarClienteRequest request, CancellationToken ct) => oficina.AlterarClienteAsync(id, request.Nome, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken ct)
    {
        await oficina.InativarClienteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reativacao")]
    public Task<ClienteDto> Reativar(Guid id, CancellationToken ct) => oficina.ReativarClienteAsync(id, ct);
}

public sealed record CriarClienteRequest(string Nome, string DocumentoFiscal);
public sealed record AlterarClienteRequest(string Nome);
