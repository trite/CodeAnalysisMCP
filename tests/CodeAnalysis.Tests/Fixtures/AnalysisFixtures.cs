using CodeAnalysis.Core.Models;
using CodeAnalysis.Core.Services;
using Microsoft.CodeAnalysis;
using Xunit;

namespace CodeAnalysis.Tests.Fixtures;

/// <summary>
/// Shared fixture for WithUnreferencedCode project analysis.
/// Analyzed once and shared across all tests in the collection.
/// </summary>
public class WithUnreferencedCodeFixture : IAsyncLifetime
{
    public AnalysisResult Result { get; private set; } = null!;
    public Project Project { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var loader = new WorkspaceLoader(TestHelpers.CreateNullLogger<WorkspaceLoader>());
        var analyzer = new UnreferencedCodeAnalyzer(TestHelpers.CreateNullLogger<UnreferencedCodeAnalyzer>());

        var projectPath = TestHelpers.GetTestDataPath("WithUnreferencedCode/WithUnreferencedCode.csproj");
        Project = await loader.LoadProjectAsync(projectPath);

        var options = new AnalysisOptions
        {
            IncludePublicMembers = true,
            IncludeEntryPoints = false
        };

        Result = await analyzer.AnalyzeProjectAsync(Project, options);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

/// <summary>
/// Shared fixture for WithoutUnreferencedCode project analysis.
/// </summary>
public class WithoutUnreferencedCodeFixture : IAsyncLifetime
{
    public AnalysisResult ResultWithEntryPoints { get; private set; } = null!;
    public AnalysisResult ResultWithoutEntryPoints { get; private set; } = null!;
    public Project Project { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var loader = new WorkspaceLoader(TestHelpers.CreateNullLogger<WorkspaceLoader>());
        var analyzer = new UnreferencedCodeAnalyzer(TestHelpers.CreateNullLogger<UnreferencedCodeAnalyzer>());

        var projectPath = TestHelpers.GetTestDataPath("WithoutUnreferencedCode/WithoutUnreferencedCode.csproj");
        Project = await loader.LoadProjectAsync(projectPath);

        // Analyze with entry points included
        var optionsWithEntryPoints = new AnalysisOptions
        {
            IncludePublicMembers = true,
            IncludeEntryPoints = true
        };
        ResultWithEntryPoints = await analyzer.AnalyzeProjectAsync(Project, optionsWithEntryPoints);

        // Analyze without entry points
        var optionsWithoutEntryPoints = new AnalysisOptions
        {
            IncludePublicMembers = true,
            IncludeEntryPoints = false
        };
        ResultWithoutEntryPoints = await analyzer.AnalyzeProjectAsync(Project, optionsWithoutEntryPoints);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

/// <summary>
/// Shared fixture for MixedAccessibility project analysis.
/// </summary>
public class MixedAccessibilityFixture : IAsyncLifetime
{
    public AnalysisResult ResultWithPublic { get; private set; } = null!;
    public AnalysisResult ResultWithoutPublic { get; private set; } = null!;
    public Project Project { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var loader = new WorkspaceLoader(TestHelpers.CreateNullLogger<WorkspaceLoader>());
        var analyzer = new UnreferencedCodeAnalyzer(TestHelpers.CreateNullLogger<UnreferencedCodeAnalyzer>());

        var projectPath = TestHelpers.GetTestDataPath("MixedAccessibility/MixedAccessibility.csproj");
        Project = await loader.LoadProjectAsync(projectPath);

        // Analyze with public members included
        var optionsWithPublic = new AnalysisOptions
        {
            IncludePublicMembers = true,
            IncludeEntryPoints = false
        };
        ResultWithPublic = await analyzer.AnalyzeProjectAsync(Project, optionsWithPublic);

        // Analyze without public members
        var optionsWithoutPublic = new AnalysisOptions
        {
            IncludePublicMembers = false,
            IncludeEntryPoints = false
        };
        ResultWithoutPublic = await analyzer.AnalyzeProjectAsync(Project, optionsWithoutPublic);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

/// <summary>
/// Shared fixture for TestData solution analysis.
/// </summary>
public class TestDataSolutionFixture : IAsyncLifetime
{
    public AnalysisResult FullResult { get; private set; } = null!;
    public AnalysisResult FilteredResult { get; private set; } = null!;
    public Solution Solution { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var loader = new WorkspaceLoader(TestHelpers.CreateNullLogger<WorkspaceLoader>());
        var analyzer = new UnreferencedCodeAnalyzer(TestHelpers.CreateNullLogger<UnreferencedCodeAnalyzer>());

        var solutionPath = TestHelpers.GetTestDataPath("TestData.sln");
        Solution = await loader.LoadSolutionAsync(solutionPath);

        // Full solution analysis
        var fullOptions = new AnalysisOptions
        {
            IncludePublicMembers = true,
            IncludeEntryPoints = false
        };
        FullResult = await analyzer.AnalyzeSolutionAsync(Solution, fullOptions);

        // Filtered analysis (only WithUnreferencedCode)
        var filteredOptions = new AnalysisOptions
        {
            IncludePublicMembers = true,
            IncludeEntryPoints = false,
            ProjectFilter = new List<string> { "WithUnreferencedCode" }
        };
        FilteredResult = await analyzer.AnalyzeSolutionAsync(Solution, filteredOptions);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

/// <summary>
/// Shared fixture for WorkspaceLoader tests.
/// </summary>
public class WorkspaceLoaderFixture : IAsyncLifetime
{
    public Project WithUnreferencedCodeProject { get; private set; } = null!;
    public Solution TestDataSolution { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var loader = new WorkspaceLoader(TestHelpers.CreateNullLogger<WorkspaceLoader>());

        var projectPath = TestHelpers.GetTestDataPath("WithUnreferencedCode/WithUnreferencedCode.csproj");
        WithUnreferencedCodeProject = await loader.LoadProjectAsync(projectPath);

        var solutionPath = TestHelpers.GetTestDataPath("TestData.sln");
        TestDataSolution = await loader.LoadSolutionAsync(solutionPath);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
