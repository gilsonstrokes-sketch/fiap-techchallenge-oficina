using Oficina.Domain;

namespace Oficina.Domain.Tests;

public sealed class DependencyBoundaryTests
{
    [Fact]
    public void DomainAssembly_DoesNotReferenceOuterLayersOrFrameworks()
    {
        var forbiddenPrefixes = new[]
        {
            "Oficina.Application",
            "Oficina.Infrastructure",
            "Oficina.Api",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "Npgsql"
        };

        var forbiddenReferences = typeof(AssemblyMarker).Assembly
            .GetReferencedAssemblies()
            .Where(reference => forbiddenPrefixes.Any(prefix =>
                reference.Name?.StartsWith(prefix, StringComparison.Ordinal) == true))
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Empty(forbiddenReferences);
    }
}
