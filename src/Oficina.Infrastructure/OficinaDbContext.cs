using Microsoft.EntityFrameworkCore;

namespace Oficina.Infrastructure;

public sealed class OficinaDbContext(DbContextOptions<OficinaDbContext> options) : DbContext(options)
{
}
