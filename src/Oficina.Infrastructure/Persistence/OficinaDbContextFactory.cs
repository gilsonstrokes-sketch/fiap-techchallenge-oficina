using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Oficina.Infrastructure.Persistence;

public sealed class OficinaDbContextFactory : IDesignTimeDbContextFactory<OficinaDbContext>
{
    public OficinaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=oficina;Username=oficina";
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new OficinaDbContext(options);
    }
}
