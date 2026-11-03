using System.Net;
using System.Net.Http.Json;

namespace Oficina.Integration.Tests;

public sealed class AuthenticationApiTests(HealthApiFactory factory) : IClassFixture<HealthApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task AdminEndpoint_ExigeToken()
    {
        using var response = await _client.GetAsync("/api/clientes");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoginAdmin_EmiteJwtSemDevolverSenha()
    {
        using var response = await _client.PostAsJsonAsync("/api/auth/admin", new
        {
            usuario = "admin-test",
            senha = "integration-test-only-password"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(token);
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
        Assert.True(token.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task LoginAdmin_ComSenhaIncorreta_RetornaUnauthorized()
    {
        using var response = await _client.PostAsJsonAsync("/api/auth/admin", new
        {
            usuario = "admin-test",
            senha = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAt);
}
