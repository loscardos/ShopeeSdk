using ShopeeSdk.Generator;

var schemaDirectory = args.Length > 0 ? args[0] : Path.Combine("reference", "schemas");
var reader = new SchemaReader();
var endpoints = reader.ReadDirectory(schemaDirectory);
var writer = new ContractWriter();
Console.WriteLine(writer.WriteSummary(endpoints));
