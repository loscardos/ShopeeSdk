using System.Text.RegularExpressions;

namespace Loscardos.ShopeeSdk.SampleConsole;

public enum EndpointClassification
{
    Readonly,
    Mutation
}

public enum EndpointCertificationStatus
{
    ReadonlyCertified,
    ReadonlyNeedsLiveCertification,
    MutationNeedsFixture
}

public sealed record GeneratedEndpoint(
    string Area,
    string Method,
    string Path,
    string AuthScope,
    string MethodName = "",
    string RequestTypeName = "");

public sealed record EndpointCertificationRow(
    string Area,
    string Method,
    string Path,
    string AuthScope,
    EndpointClassification Classification,
    EndpointCertificationStatus Status,
    bool RequiresFixture,
    string Detail);

public static class GeneratedEndpointInventory
{
    public static string DefaultGeneratedManagersDirectory => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "..",
        "src",
        "ShopeeSdk.Core",
        "Managers",
        "Generated"));

    public static IReadOnlyList<GeneratedEndpoint> ReadFromDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Generated manager directory was not found: {directory}");
        }

        return Directory.GetFiles(directory, "*Api.g.cs")
            .OrderBy(static path => path, StringComparer.Ordinal)
            .SelectMany(ReadFromFile)
            .OrderBy(static endpoint => endpoint.Area, StringComparer.Ordinal)
            .ThenBy(static endpoint => endpoint.Path, StringComparer.Ordinal)
            .ThenBy(static endpoint => endpoint.Method, StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<GeneratedEndpoint> ReadFromFile(string path)
    {
        var source = File.ReadAllText(path);
        var classMatch = Regex.Match(source, @"public sealed class (?<area>[A-Za-z0-9]+)Api\(");
        if (!classMatch.Success)
        {
            yield break;
        }

        var area = classMatch.Groups["area"].Value;
        var methodMatches = Regex.Matches(source, @"public async Task<");
        for (var index = 0; index < methodMatches.Count; index++)
        {
            var start = methodMatches[index].Index;
            var end = index + 1 < methodMatches.Count ? methodMatches[index + 1].Index : source.Length;
            var block = source[start..end];
            var signature = Regex.Match(block, @"public async Task<[^>]+>\s+(?<name>[\p{L}\p{N}_]+)\([^)]*?(?<request>[A-Za-z0-9]+Request)\s+request");
            var method = Regex.Match(block, @"ShopeeRequestMethod\.(?<method>Get|Post)");
            var endpointPath = Regex.Match(block, @"new ShopeeRequest\([^,]+,\s*""(?<path>[^""]+)""");
            if (!signature.Success || !method.Success || !endpointPath.Success)
            {
                continue;
            }

            yield return new GeneratedEndpoint(
                area,
                method.Groups["method"].Value.ToUpperInvariant(),
                endpointPath.Groups["path"].Value,
                ResolveAuthScope(block),
                signature.Groups["name"].Value,
                signature.Groups["request"].Value);
        }
    }

    private static string ResolveAuthScope(string block)
    {
        if (block.Contains("new ShopeeRequestAuth(shopId", StringComparison.Ordinal))
        {
            return "Shop";
        }

        if (block.Contains("new ShopeeRequestAuth(null, merchantId", StringComparison.Ordinal)
            || block.Contains("new ShopeeRequestAuth(merchantId", StringComparison.Ordinal))
        {
            return "Merchant";
        }

        return "Public";
    }
}

public static class ReadonlyCertificationCatalog
{
    public static IReadOnlySet<string> CertifiedEndpoints { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        Key("GET", "/product/get_category"),
        Key("GET", "/shop/get_shop_info"),
        Key("GET", "/order/get_order_list"),
        Key("GET", "/order/get_order_detail"),
        Key("GET", "/payment/get_escrow_detail"),
        Key("GET", "/logistics/get_tracking_info")
    };

    public static string Key(string method, string path)
        => $"{method.ToUpperInvariant()} {path}";
}

public static class EndpointCertificationMatrix
{
    public static string DefaultPath => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "..",
        ".tmp",
        "shopee-endpoint-certification-matrix.md"));

    public static IReadOnlyList<EndpointCertificationRow> Create(
        IReadOnlyList<GeneratedEndpoint> endpoints,
        IReadOnlySet<string> readonlyCertifiedEndpoints)
        => endpoints.Select(endpoint =>
        {
            var classification = Classify(endpoint);
            var status = ResolveStatus(endpoint, classification, readonlyCertifiedEndpoints);
            return new EndpointCertificationRow(
                endpoint.Area,
                endpoint.Method,
                endpoint.Path,
                endpoint.AuthScope,
                classification,
                status,
                status == EndpointCertificationStatus.MutationNeedsFixture,
                ResolveDetail(status));
        }).ToArray();

    public static void WriteMarkdown(string path, IReadOnlyList<EndpointCertificationRow> rows)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, ToMarkdown(rows));
    }

    public static string ToMarkdown(IReadOnlyList<EndpointCertificationRow> rows)
    {
        var lines = new List<string>
        {
            "# Shopee SDK Endpoint Certification Matrix",
            string.Empty,
            $"Generated at: {DateTimeOffset.UtcNow:O}",
            string.Empty,
            $"Total endpoints: {rows.Count}",
            string.Empty,
            "| Status | Count |",
            "| --- | ---: |"
        };

        foreach (var group in rows.GroupBy(static row => row.Status).OrderBy(static group => group.Key.ToString()))
        {
            lines.Add($"| {group.Key} | {group.Count()} |");
        }

        lines.AddRange([
            string.Empty,
            "| Area | Method | Path | Auth | Class | Status | Fixture | Detail |",
            "| --- | --- | --- | --- | --- | --- | --- | --- |"
        ]);

        foreach (var row in rows.OrderBy(static row => row.Area, StringComparer.Ordinal).ThenBy(static row => row.Path, StringComparer.Ordinal).ThenBy(static row => row.Method, StringComparer.Ordinal))
        {
            lines.Add($"| {Escape(row.Area)} | {row.Method} | {Escape(row.Path)} | {Escape(row.AuthScope)} | {row.Classification} | {row.Status} | {YesNo(row.RequiresFixture)} | {Escape(row.Detail)} |");
        }

        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    public static EndpointClassification Classify(GeneratedEndpoint endpoint)
    {
        if (endpoint.Method == "GET")
        {
            return EndpointClassification.Readonly;
        }

        var path = endpoint.Path.ToLowerInvariant();
        return path.Contains("/get_", StringComparison.Ordinal)
            || path.Contains("/search", StringComparison.Ordinal)
            || path.Contains("/list", StringComparison.Ordinal)
            || path.Contains("/query", StringComparison.Ordinal)
            || path.Contains("/check", StringComparison.Ordinal)
            || path.Contains("/download", StringComparison.Ordinal)
            ? EndpointClassification.Readonly
            : EndpointClassification.Mutation;
    }

    private static EndpointCertificationStatus ResolveStatus(
        GeneratedEndpoint endpoint,
        EndpointClassification classification,
        IReadOnlySet<string> readonlyCertifiedEndpoints)
    {
        if (readonlyCertifiedEndpoints.Contains(ReadonlyCertificationCatalog.Key(endpoint.Method, endpoint.Path)))
        {
            return EndpointCertificationStatus.ReadonlyCertified;
        }

        return classification == EndpointClassification.Mutation
            ? EndpointCertificationStatus.MutationNeedsFixture
            : EndpointCertificationStatus.ReadonlyNeedsLiveCertification;
    }

    private static string ResolveDetail(EndpointCertificationStatus status)
        => status switch
        {
            EndpointCertificationStatus.ReadonlyCertified => "covered by readonly certification command",
            EndpointCertificationStatus.ReadonlyNeedsLiveCertification => "readonly endpoint needs sandbox/live certification",
            EndpointCertificationStatus.MutationNeedsFixture => "mutation endpoint needs dedicated sandbox fixture and explicit opt-in",
            _ => status.ToString()
        };

    private static string Escape(string value)
        => value.Replace("|", "\\|", StringComparison.Ordinal).ReplaceLineEndings(" ");

    private static string YesNo(bool value) => value ? "yes" : "no";
}
