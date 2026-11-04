using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application;
using Oficina.Application.Models;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize(Policy = "Administrador")]
[Route("api/veiculos")]
public sealed class VeiculosController(OficinaApplicationService oficina) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<VeiculoDto>> Listar([FromQuery] Guid? clienteId, CancellationToken ct) => oficina.ListarVeiculosAsync(clienteId, ct);

    [HttpGet("{id:guid}")]
    public Task<VeiculoDto> Obter(Guid id, CancellationToken ct) => oficina.ObterVeiculoAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<VeiculoDto>> Criar([FromBody] CriarVeiculoRequest request, CancellationToken ct)
    {
        var veiculo = await oficina.CriarVeiculoAsync(request.ClienteId, request.Placa, request.Marca, request.Modelo, request.Ano, ct);
        return CreatedAtAction(nameof(Obter), new { id = veiculo.Id }, veiculo);
    }

    [HttpPut("{id:guid}")]
    public Task<VeiculoDto> Alterar(Guid id, [FromBody] AlterarVeiculoRequest request, CancellationToken ct) =>
        oficina.AlterarVeiculoAsync(id, request.Placa, request.Marca, request.Modelo, request.Ano, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken ct)
    {
        await oficina.InativarVeiculoAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reativacao")]
    public Task<VeiculoDto> Reativar(Guid id, CancellationToken ct) => oficina.ReativarVeiculoAsync(id, ct);
}

public sealed record CriarVeiculoRequest(Guid ClienteId, string Placa, string Marca, string Modelo, int Ano);
public sealed record AlterarVeiculoRequest(string Placa, string Marca, string Modelo, int Ano);
