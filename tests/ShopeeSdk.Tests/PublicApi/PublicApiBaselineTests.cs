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
        var current = GetPublicTypeLines();

        UpdateBaselineWhenRequested("UPDATE_PUBLIC_API_BASELINE", baselinePath, current);

        File.Exists(baselinePath).Should().BeTrue("public type drift should be reviewed explicitly");
        File.ReadAllLines(baselinePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Should()
            .Equal(current);
    }

    [Fact]
    public void ShippingAssemblies_MatchPublicMemberBaseline()
    {
        var baselinePath = Path.Combine(FindRepoRoot(), "tests", "ShopeeSdk.Tests", "PublicApi", "public-api-members-baseline.txt");
        var current = GetPublicMemberLines();

        UpdateBaselineWhenRequested("UPDATE_PUBLIC_API_MEMBERS_BASELINE", baselinePath, current);

        File.Exists(baselinePath).Should().BeTrue("public member drift should be reviewed explicitly");
        File.ReadAllLines(baselinePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Should()
            .Equal(current);
    }

    private static string[] GetPublicTypeLines()
    {
        return ShippingAssemblies
            .OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal)
            .SelectMany(assembly => assembly.ExportedTypes
                .Where(type => !IsCompilerGenerated(type))
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .Select(type => $"{assembly.GetName().Name}:{GetTypeDisplayName(type)}"))
            .ToArray();
    }

    private static string[] GetPublicMemberLines()
    {
        return ShippingAssemblies
            .OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal)
            .SelectMany(assembly => assembly.ExportedTypes
                .Where(type => !IsCompilerGenerated(type))
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .SelectMany(type => GetPublicMembers(type)
                    .Select(member => $"{assembly.GetName().Name}:{GetTypeDisplayName(type)}.{member}")))
            .ToArray();
    }

    private static IEnumerable<string> GetPublicMembers(Type type)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly;

        foreach (var constructor in type.GetConstructors(flags).OrderBy(GetConstructorSignature, StringComparer.Ordinal))
        {
            yield return GetConstructorSignature(constructor);
        }

        foreach (var property in type.GetProperties(flags).OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            yield return $"property {GetTypeName(property.PropertyType)} {property.Name}";
        }

        foreach (var method in type.GetMethods(flags)
            .Where(method => !method.IsSpecialName)
            .OrderBy(GetMethodSignature, StringComparer.Ordinal))
        {
            yield return GetMethodSignature(method);
        }
    }

    private static string GetConstructorSignature(ConstructorInfo constructor)
    {
        return $".ctor({string.Join(", ", constructor.GetParameters().Select(GetParameterSignature))})";
    }

    private static string GetMethodSignature(MethodInfo method)
    {
        return $"method {GetTypeName(method.ReturnType)} {method.Name}({string.Join(", ", method.GetParameters().Select(GetParameterSignature))})";
    }

    private static string GetParameterSignature(ParameterInfo parameter)
    {
        var defaultValue = parameter.HasDefaultValue ? " = default" : string.Empty;
        return $"{GetTypeName(parameter.ParameterType)} {parameter.Name}{defaultValue}";
    }

    private static string GetTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.FullName ?? type.Name;
        }

        var genericName = type.GetGenericTypeDefinition().FullName ?? type.Name;
        var tickIndex = genericName.IndexOf('`', StringComparison.Ordinal);
        if (tickIndex >= 0)
        {
            genericName = genericName[..tickIndex];
        }

        return genericName + "<" + string.Join(", ", type.GetGenericArguments().Select(GetTypeName)) + ">";
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

    private static void UpdateBaselineWhenRequested(string variableName, string baselinePath, IReadOnlyList<string> current)
    {
        if (Environment.GetEnvironmentVariable(variableName) == "1")
        {
            File.WriteAllText(baselinePath, string.Join(Environment.NewLine, current) + Environment.NewLine);
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
