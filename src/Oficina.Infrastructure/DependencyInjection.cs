using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oficina.Application.Abstractions.Persistence;
using Oficina.Infrastructure.Persistence;

namespace Oficina.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return services;
        }

        services.AddDbContext<OficinaDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IClienteRepository, EfClienteRepository>();
        services.AddScoped<IVeiculoRepository, EfVeiculoRepository>();
        services.AddScoped<IServicoRepository, EfServicoRepository>();
        services.AddScoped<IPecaRepository, EfPecaRepository>();
        services.AddScoped<IOrdemDeServicoRepository, EfOrdemDeServicoRepository>();
        services.AddScoped<IEstoqueDaPecaRepository, EfEstoqueDaPecaRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
