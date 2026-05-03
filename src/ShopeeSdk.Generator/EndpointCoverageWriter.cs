using System.Text;

namespace ShopeeSdk.Generator;

public sealed class EndpointCoverageWriter
{
    public string WriteMarkdown(IReadOnlyList<SchemaEndpoint> endpoints)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Endpoint Coverage");
        builder.AppendLine();
        builder.AppendLine($"Total schema endpoints: {endpoints.Count}");
        builder.AppendLine();
        builder.AppendLine("This report reflects the Shopee schema snapshot used by the generator. Endpoint availability can still vary by Shopee region, shop permissions, merchant authorization, and Shopee platform rollout state.");
        builder.AppendLine();
        builder.AppendLine("| Module | Endpoints |");
        builder.AppendLine("| --- | ---: |");

        foreach (var group in endpoints.GroupBy(endpoint => endpoint.ModuleName).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            builder.AppendLine($"| {group.Key} | {group.Count()} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Endpoint List");
        builder.AppendLine();

        foreach (var endpoint in endpoints.OrderBy(endpoint => endpoint.Path, StringComparer.Ordinal))
        {
            builder.AppendLine($"- `{endpoint.Path}` `{endpoint.HttpMethod}` `{endpoint.AuthScope}` `{endpoint.RequestContentKind}`");
        }

        return builder.ToString();
    }
}
