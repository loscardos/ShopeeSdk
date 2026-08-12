using System.Xml.Linq;
using FluentAssertions;

namespace Loscardos.ShopeeSdk.Tests.Packaging;

public sealed class PackageDistributionGuardrailTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void Shipping_projects_use_the_Loscardos_package_assembly_and_namespace_identity()
    {
        var props = XDocument.Load(Path.Combine(RepositoryRoot, "Directory.Build.props"));
        var properties = props
            .Descendants("PropertyGroup")
            .Elements()
            .ToDictionary(element => element.Name.LocalName, element => element.Value);

        properties["PackageId"].Should().Be("Loscardos.$(MSBuildProjectName)");
        properties["AssemblyName"].Should().Be("Loscardos.$(MSBuildProjectName)");
        properties["RootNamespace"].Should().Be("Loscardos.$(MSBuildProjectName)");

        var namespaceViolations = Directory
            .EnumerateFiles(Path.Combine(RepositoryRoot, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => File.ReadLines(path).Any(line =>
                line.TrimStart().StartsWith("namespace ShopeeSdk", StringComparison.Ordinal)))
            .Select(path => Path.GetRelativePath(RepositoryRoot, path))
            .ToArray();

        namespaceViolations.Should().BeEmpty();
    }

    [Fact]
    public void Production_push_publishes_private_packages_without_retesting()
    {
        var workflowPath = Path.Combine(RepositoryRoot, ".github", "workflows", "package.yml");
        File.Exists(workflowPath).Should().BeTrue();

        var workflow = File.ReadAllText(workflowPath);
        workflow.Should().Contain("branches:");
        workflow.Should().Contain("- production");
        workflow.Should().Contain("packages: write");
        workflow.Should().Contain("https://nuget.pkg.github.com/loscardos/index.json");
        workflow.Should().Contain("dotnet nuget push");
        workflow.Should().NotContain("dotnet test");
        workflow.Should().NotContain("--skip-duplicate");
    }

    private static string FindRepositoryRoot()
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

        throw new DirectoryNotFoundException("Could not find the ShopeeSdk repository root.");
    }
}
