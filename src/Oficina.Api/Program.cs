using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Oficina.Api.Security;
using Oficina.Application;
using Oficina.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeDocumentTransformer>();
    options.AddOperationTransformer<AuthorizeMetadataOperationTransformer>();
});
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);
if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Postgres")))
    builder.Services.AddScoped<OficinaApplicationService>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSingleton(sp =>
{
    var configuredKey = sp.GetRequiredService<IConfiguration>()["Authentication:SigningKey"];
    ExigirChaveJwt(configuredKey);
    return new OficinaTokenService(configuredKey!);
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var configuredKey = builder.Configuration["Authentication:SigningKey"];
        ExigirChaveJwt(configuredKey);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = OficinaTokenService.Issuer,
            ValidateAudience = true,
            ValidAudience = OficinaTokenService.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuredKey!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireAuthenticatedUser().RequireRole(OficinaTokenService.AdminRole));
    options.AddPolicy("ConsultaDaOrdem", policy => policy.RequireAuthenticatedUser().RequireClaim("scope", OficinaTokenService.ClientScope));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Oficina API v1"));
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

static void ExigirChaveJwt(string? key)
{
    if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
        throw new InvalidOperationException("Configure Authentication__SigningKey com ao menos 32 bytes via variável de ambiente ou user-secrets.");
}

public partial class Program
{
}
