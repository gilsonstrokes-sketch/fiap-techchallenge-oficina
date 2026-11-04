using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application;
using Oficina.Application.Models;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize(Policy = "Administrador")]
[Route("api/estoque")]
public sealed class EstoqueController(OficinaApplicationService oficina) : ControllerBase
{
    [HttpGet("pecas/{pecaId:guid}")]
    public Task<EstoqueDto> Obter(Guid pecaId, CancellationToken ct) => oficina.ObterEstoqueAsync(pecaId, ct);

    [HttpPost("pecas/{pecaId:guid}/entradas")]
    public Task<EstoqueDto> RegistrarEntrada(Guid pecaId, [FromBody] EntradaEstoqueRequest request, CancellationToken ct) => oficina.RegistrarEntradaEstoqueAsync(pecaId, request.Quantidade, ct);
}

public sealed record EntradaEstoqueRequest(decimal Quantidade);
