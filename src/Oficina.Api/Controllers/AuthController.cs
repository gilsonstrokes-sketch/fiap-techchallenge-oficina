using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Api.Security;
using Oficina.Application;
using Oficina.Domain.ValueObjects;

namespace Oficina.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IConfiguration configuration,
    OficinaTokenService tokens,
    IServiceScopeFactory scopeFactory) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("admin")]
    public IActionResult EntrarAdministrador([FromBody] LoginAdministradorRequest request)
    {
        var usuarioConfigurado = configuration["Authentication:AdminUsername"];
        var senhaConfigurada = configuration["Authentication:AdminPassword"];
        if (string.IsNullOrEmpty(usuarioConfigurado) || string.IsNullOrEmpty(senhaConfigurada)
            || !string.Equals(usuarioConfigurado, request.Usuario, StringComparison.Ordinal)
            || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(senhaConfigurada), Encoding.UTF8.GetBytes(request.Senha)))
        {
            return Unauthorized();
        }

        var token = tokens.CriarTokenAdministrador(usuarioConfigurado);
        return Ok(new TokenResponse(token.Token, token.ExpiresAt));
    }

    [AllowAnonymous]
    [HttpPost("cliente")]
    public async Task<ActionResult<TokenResponse>> CriarTokenDeConsulta([FromBody] TokenConsultaRequest request, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var oficina = scope.ServiceProvider.GetRequiredService<OficinaApplicationService>();
        await oficina.ConsultarOrdemClienteAsync(request.OrdemDeServicoId, DocumentoFiscal.Criar(request.DocumentoFiscal), cancellationToken);
        var token = tokens.CriarTokenCliente(request.OrdemDeServicoId);
        return Ok(new TokenResponse(token.Token, token.ExpiresAt));
    }
}

public sealed record LoginAdministradorRequest(string Usuario, string Senha);
public sealed record TokenConsultaRequest(Guid OrdemDeServicoId, string DocumentoFiscal);
public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAt);
