using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Oficina.Api.Security;

public sealed class OficinaTokenService(string signingKey)
{
    public const string Issuer = "fiap-oficina";
    public const string Audience = "fiap-oficina-api";
    public const string AdminRole = "Administrador";
    public const string ClientScope = "ordem:consultar";

    public (string Token, DateTimeOffset ExpiresAt) CriarTokenAdministrador(string usuario)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddHours(8);
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, usuario),
            new Claim(ClaimTypes.Role, AdminRole),
            new Claim("scope", "administracao")
        };
        return (CriarToken(claims, expiresAt), expiresAt);
    }

    public (string Token, DateTimeOffset ExpiresAt) CriarTokenCliente(Guid ordemId)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        var claims = new[]
        {
            new Claim("scope", ClientScope),
            new Claim("ordem_id", ordemId.ToString("D"))
        };
        return (CriarToken(claims, expiresAt), expiresAt);
    }

    private string CriarToken(IEnumerable<Claim> claims, DateTimeOffset expiresAt)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(Issuer, Audience, claims, expires: expiresAt.UtcDateTime, signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
