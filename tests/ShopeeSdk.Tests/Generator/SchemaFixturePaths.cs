namespace ShopeeSdk.Tests.Generator;

internal static class SchemaFixturePaths
{
    public static string FindSchemas()
    {
        var repoRoot = FindRepoRoot();
        return Path.Combine(repoRoot, "tests", "ShopeeSdk.Tests", "Fixtures", "Schemas");
    }

    public static string FindRepoRoot()
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
