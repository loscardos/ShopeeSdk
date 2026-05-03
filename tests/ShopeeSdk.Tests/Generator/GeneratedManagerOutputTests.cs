using FluentAssertions;
using ShopeeSdk.Generator;

namespace ShopeeSdk.Tests.Generator;

public sealed class GeneratedManagerOutputTests
{
    [Fact]
    public void GeneratedManagerInterfaces_MatchReferenceSchemas()
    {
        var repoRoot = FindRepoRoot();
        var endpoints = new SchemaReader().ReadDirectory(Path.Combine(repoRoot, "reference", "schemas"));
        var writer = new ManagersWriter();

        foreach (var module in endpoints.GroupBy(endpoint => endpoint.ModuleKey).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var expected = writer.WriteInterface(module.First().ModuleName, module.Key, module.ToArray());
            var moduleName = ShopeeName.ToPascalCase(module.Key);
            var path = Path.Combine(repoRoot, "src", "ShopeeSdk.Abstractions", "Managers", "Generated", $"I{moduleName}Api.g.cs");

            File.ReadAllText(path).Should().Be(expected, $"{path} must be regenerated from reference/schemas");
        }
    }

    [Fact]
    public void GeneratedManagerImplementations_MatchReferenceSchemas()
    {
        var repoRoot = FindRepoRoot();
        var endpoints = new SchemaReader().ReadDirectory(Path.Combine(repoRoot, "reference", "schemas"));
        var writer = new ManagersWriter();

        foreach (var module in endpoints.GroupBy(endpoint => endpoint.ModuleKey).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var expected = writer.WriteImplementation(module.First().ModuleName, module.Key, module.ToArray());
            var moduleName = ShopeeName.ToPascalCase(module.Key);
            var path = Path.Combine(repoRoot, "src", "ShopeeSdk.Core", "Managers", "Generated", $"{moduleName}Api.g.cs");

            File.ReadAllText(path).Should().Be(expected, $"{path} must be regenerated from reference/schemas");
        }
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
