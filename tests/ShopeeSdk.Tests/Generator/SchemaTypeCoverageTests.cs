using FluentAssertions;
using ShopeeSdk.Generator;

namespace ShopeeSdk.Tests.Generator;

public sealed class SchemaTypeCoverageTests
{
    [Fact]
    public void ReferenceSchemas_OnlyUseKnownScalarTypesOrObjectShapes()
    {
        var endpoints = new SchemaReader().ReadDirectory(FindReferenceSchemas());
        var unknownScalarTypes = endpoints
            .SelectMany(endpoint => endpoint.RequestParameters.Concat(endpoint.ResponseParameters))
            .SelectMany(Flatten)
            .Where(parameter => parameter.Children.Count == 0)
            .Select(parameter => parameter.Type)
            .Distinct(StringComparer.Ordinal)
            .Where(type => !IsKnownScalarType(type))
            .OrderBy(type => type, StringComparer.Ordinal)
            .ToArray();

        unknownScalarTypes.Should().BeEmpty("unknown scalar types are emitted as JsonElement and must be reviewed before publishing");
    }

    [Fact]
    public void ReferenceSchemas_StillHaveExpectedObjectFallbacks()
    {
        var endpoints = new SchemaReader().ReadDirectory(FindReferenceSchemas());
        var objectFallbackCount = endpoints
            .SelectMany(endpoint => endpoint.RequestParameters.Concat(endpoint.ResponseParameters))
            .SelectMany(Flatten)
            .Count(parameter => parameter.Children.Count == 0 && parameter.Type == "object");

        objectFallbackCount.Should().BeGreaterThan(0);
    }

    private static IEnumerable<SchemaParameter> Flatten(SchemaParameter parameter)
    {
        yield return parameter;

        foreach (var child in parameter.Children.SelectMany(Flatten))
        {
            yield return child;
        }
    }

    private static bool IsKnownScalarType(string type)
    {
        return type is
            "string" or
            "int" or
            "int32" or
            "integer" or
            "int64" or
            "long" or
            "timestamp" or
            "float" or
            "double" or
            "decimal" or
            "boolean" or
            "bool" or
            "string[]" or
            "int[]" or
            "int32[]" or
            "int64[]" or
            "float[]" or
            "double[]" or
            "decimal[]" or
            "file" or
            "object" or
            "object[]";
    }

    private static string FindReferenceSchemas()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "reference", "schemas");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find reference/schemas from test output directory.");
    }
}
