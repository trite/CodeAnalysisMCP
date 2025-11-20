using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeAnalysis.Tests;

public static class TestHelpers
{
    public static string GetTestDataPath(string relativePath)
    {
        // Get the path relative to the test assembly
        var assemblyLocation = typeof(TestHelpers).Assembly.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation)!;
        return Path.Combine(assemblyDirectory, "TestData", relativePath);
    }

    public static ILogger<T> CreateNullLogger<T>()
    {
        return NullLogger<T>.Instance;
    }
}
