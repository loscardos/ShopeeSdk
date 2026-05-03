using System.Reflection;
using FluentAssertions;
using ShopeeSdk.Abstractions.Configuration;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Generated.Product;

namespace ShopeeSdk.Tests.PublicApi;

public sealed class PublicApiBaselineTests
{
    private static readonly Assembly[] ShippingAssemblies =
    [
        typeof(ShopeeOptions).Assembly,
        typeof(ShopeeClient).Assembly,
        typeof(ProductGetCategoryRequest).Assembly
    ];

    [Fact]
    public void ShippingAssemblies_MatchPublicTypeBaseline()
    {
        var baselinePath = Path.Combine(FindRepoRoot(), "tests", "ShopeeSdk.Tests", "PublicApi", "public-api-baseline.txt");
        var current = GetPublicApiLines();

        if (Environment.GetEnvironmentVariable("UPDATE_PUBLIC_API_BASELINE") == "1")
        {
            File.WriteAllText(baselinePath, string.Join(Environment.NewLine, current) + Environment.NewLine);
        }

        File.Exists(baselinePath).Should().BeTrue("public API drift should be reviewed explicitly");

        var expected = File.ReadAllLines(baselinePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        current.Should().Equal(expected);
    }

    private static string[] GetPublicApiLines()
    {
        return ShippingAssemblies
            .OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal)
            .SelectMany(assembly => assembly.ExportedTypes
                .Where(type => !IsCompilerGenerated(type))
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .Select(type => $"{assembly.GetName().Name}:{GetTypeDisplayName(type)}"))
            .ToArray();
    }

    private static bool IsCompilerGenerated(Type type)
    {
        return type.FullName?.Contains("<", StringComparison.Ordinal) == true;
    }

    private static string GetTypeDisplayName(Type type)
    {
        var name = type.FullName ?? type.Name;
        var tickIndex = name.IndexOf('`', StringComparison.Ordinal);
        return tickIndex < 0 ? name : name[..tickIndex];
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ShopeeSdk.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root from test output directory.");
    }
}
