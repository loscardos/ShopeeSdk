namespace Loscardos.ShopeeSdk.SampleConsole;

public enum CertificationStatus
{
    Pass,
    Fail,
    SkipNoData,
    SkipScope,
    SkipMutationNeedsFixture
}

public sealed record CertificationResult(
    string Area,
    string Endpoint,
    CertificationStatus Status,
    string Detail);

public static class CertificationReport
{
    public static string DefaultPath => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "..",
        ".tmp",
        "shopee-readonly-certification.md"));

    public static void WriteMarkdown(string path, IReadOnlyList<CertificationResult> results)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, ToMarkdown(results));
    }

    public static string ToMarkdown(IReadOnlyList<CertificationResult> results)
    {
        var lines = new List<string>
        {
            "# Shopee SDK Readonly Certification",
            string.Empty,
            $"Generated at: {DateTimeOffset.UtcNow:O}",
            string.Empty,
            "| Status | Count |",
            "| --- | ---: |"
        };

        foreach (var group in results.GroupBy(static result => result.Status).OrderBy(static group => group.Key.ToString()))
        {
            lines.Add($"| {group.Key} | {group.Count()} |");
        }

        lines.AddRange([
            string.Empty,
            "| Area | Endpoint | Status | Detail |",
            "| --- | --- | --- | --- |"
        ]);

        foreach (var result in results.OrderBy(static result => result.Area, StringComparer.Ordinal).ThenBy(static result => result.Endpoint, StringComparer.Ordinal))
        {
            lines.Add($"| {Escape(result.Area)} | {Escape(result.Endpoint)} | {result.Status} | {Escape(result.Detail)} |");
        }

        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static string Escape(string value)
        => value.Replace("|", "\\|", StringComparison.Ordinal).ReplaceLineEndings(" ");
}
