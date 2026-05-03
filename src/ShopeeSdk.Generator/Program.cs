using ShopeeSdk.Generator;

var mode = args.Length > 0 ? args[0] : "summary";
var schemaDirectory = args.Length > 1 ? args[1] : Path.Combine("tests", "ShopeeSdk.Tests", "Fixtures", "Schemas");
var outputDirectory = args.Length > 2 ? args[2] : Directory.GetCurrentDirectory();

var reader = new SchemaReader();
var endpoints = reader.ReadDirectory(schemaDirectory);

switch (mode)
{
    case "summary":
        Console.WriteLine(new ContractWriter().WriteSummary(endpoints));
        break;
    case "coverage":
        Console.WriteLine(new EndpointCoverageWriter().WriteMarkdown(endpoints));
        break;
    case "generate":
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllText(Path.Combine(outputDirectory, "endpoint-coverage.md"), new EndpointCoverageWriter().WriteMarkdown(endpoints));

        foreach (var group in endpoints.GroupBy(endpoint => endpoint.ModuleKey).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var moduleDirectory = Path.Combine("src", "ShopeeSdk.Generated", ShopeeName.ToPascalCase(group.Key));
            Directory.CreateDirectory(moduleDirectory);
            File.WriteAllText(
                Path.Combine(moduleDirectory, ShopeeName.ToPascalCase(group.Key) + "Contracts.g.cs"),
                new ContractsWriter().WriteModule(group.First().ModuleName, group.Key, group.ToArray()));

            var abstractionDirectory = Path.Combine("src", "ShopeeSdk.Abstractions", "Managers", "Generated");
            var coreDirectory = Path.Combine("src", "ShopeeSdk.Core", "Managers", "Generated");
            Directory.CreateDirectory(abstractionDirectory);
            Directory.CreateDirectory(coreDirectory);

            File.WriteAllText(
                Path.Combine(abstractionDirectory, ShopeeName.ToManagerInterfaceName(group.Key) + ".g.cs"),
                new ManagersWriter().WriteInterface(group.First().ModuleName, group.Key, group.ToArray()));

            File.WriteAllText(
                Path.Combine(coreDirectory, ShopeeName.ToManagerImplementationName(group.Key) + ".g.cs"),
                new ManagersWriter().WriteImplementation(group.First().ModuleName, group.Key, group.ToArray()));
        }

        break;
    default:
        Console.Error.WriteLine("Usage: dotnet run --project src/ShopeeSdk.Generator -- [summary|coverage|generate] [schemaDirectory] [outputDirectory]");
        return 2;
}

return 0;
