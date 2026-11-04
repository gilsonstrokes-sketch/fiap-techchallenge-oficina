using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Oficina.Integration.Tests;

public sealed class HealthEndpointTests : IClassFixture<HealthApiFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(HealthApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthy()
    {
        using var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task GetOpenApiDocument_ReturnsJson()
    {
        using var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetOpenApiDocument_DeclaresBearerOnlyForAuthorizedOperations()
    {
        using var response = await _client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        var bearer = root.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");
        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());
        Assert.Equal("JWT", bearer.GetProperty("bearerFormat").GetString());
        Assert.False(root.TryGetProperty("security", out _));

        AssertOperationRequiresBearer(root, "/api/clientes", "get");
        AssertOperationRequiresBearer(root, "/api/consulta/ordens-de-servico/{id}", "get");
        AssertOperationDoesNotRequireBearer(root, "/api/auth/admin", "post");
        AssertOperationDoesNotRequireBearer(root, "/api/auth/cliente", "post");
    }

    [Fact]
    public async Task GetSwaggerUi_ReturnsHtml()
    {
        using var response = await _client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    private static void AssertOperationRequiresBearer(JsonElement document, string path, string method)
    {
        var security = document.GetProperty("paths").GetProperty(path).GetProperty(method).GetProperty("security");
        Assert.Contains(security.EnumerateArray(), requirement => requirement.TryGetProperty("Bearer", out _));
    }

    private static void AssertOperationDoesNotRequireBearer(JsonElement document, string path, string method)
    {
        var operation = document.GetProperty("paths").GetProperty(path).GetProperty(method);
        Assert.False(operation.TryGetProperty("security", out _));
    }
}

public sealed class HealthApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=127.0.0.1;Port=5432;Database=oficina_test;Username=test;Password=test",
                ["Authentication:SigningKey"] = "integration-test-signing-key-at-least-32-bytes",
                ["Authentication:AdminUsername"] = "admin-test",
                ["Authentication:AdminPassword"] = "integration-test-only-password"
            }));
    }
}
