namespace ShopeeSdk.SampleConsole;

public static class SandboxEnvFile
{
    public static void UpdateTokens(string path, string accessToken, string refreshToken)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Sandbox env file was not found.", path);
        }

        var lines = File.ReadAllLines(path).ToList();
        var accessUpdated = ReplaceValue(lines, "SHOPEE_ACCESS_TOKEN", accessToken);
        var refreshUpdated = ReplaceValue(lines, "SHOPEE_REFRESH_TOKEN", refreshToken);

        if (!accessUpdated)
        {
            lines.Add("SHOPEE_ACCESS_TOKEN=" + accessToken);
        }

        if (!refreshUpdated)
        {
            lines.Add("SHOPEE_REFRESH_TOKEN=" + refreshToken);
        }

        File.WriteAllLines(path, lines);
    }

    private static bool ReplaceValue(List<string> lines, string key, string value)
    {
        var prefix = key + "=";
        for (var index = 0; index < lines.Count; index++)
        {
            if (lines[index].StartsWith(prefix, StringComparison.Ordinal))
            {
                lines[index] = prefix + value;
                return true;
            }
        }

        return false;
    }
}
