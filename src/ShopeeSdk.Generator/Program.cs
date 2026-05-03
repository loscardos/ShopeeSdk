using ShopeeSdk.Generator;

var mode = args.Length > 0 ? args[0] : "summary";
var schemaDirectory = args.Length > 1 ? args[1] : Path.Combine("reference", "schemas");
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
        break;
    default:
        Console.Error.WriteLine("Usage: dotnet run --project src/ShopeeSdk.Generator -- [summary|coverage|generate] [schemaDirectory] [outputDirectory]");
        return 2;
}

return 0;
