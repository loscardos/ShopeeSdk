namespace ShopeeSdk.Generator;

public sealed class ContractWriter
{
    public string WriteSummary(IReadOnlyList<SchemaEndpoint> endpoints)
    {
        var byModule = endpoints
            .GroupBy(endpoint => endpoint.ModuleName)
            .OrderBy(group => group.Key)
            .Select(group => $"{group.Key}: {group.Count()} endpoints");

        return string.Join(Environment.NewLine, byModule);
    }
}
