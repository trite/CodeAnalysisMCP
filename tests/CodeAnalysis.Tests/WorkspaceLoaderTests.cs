using CodeAnalysis.Core.Services;
using CodeAnalysis.Tests.Fixtures;
using Xunit;

namespace CodeAnalysis.Tests;

public class WorkspaceLoaderTests : IClassFixture<WorkspaceLoaderFixture>
{
    private readonly WorkspaceLoaderFixture _fixture;

    public WorkspaceLoaderTests(WorkspaceLoaderFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void LoadProjectAsync_ValidProject_ReturnsProject()
    {
        Assert.NotNull(_fixture.WithUnreferencedCodeProject);
        Assert.Equal("WithUnreferencedCode", _fixture.WithUnreferencedCodeProject.Name);
        Assert.True(_fixture.WithUnreferencedCodeProject.Documents.Any(), "Project should have documents");
    }

    [Fact]
    public void LoadSolutionAsync_ValidSolution_ReturnsSolution()
    {
        Assert.NotNull(_fixture.TestDataSolution);
        Assert.True(_fixture.TestDataSolution.Projects.Any(), "Solution should have projects");
    }

    [Fact]
    public async Task LoadProjectAsync_InvalidPath_ThrowsFileNotFoundException()
    {
        var loader = new WorkspaceLoader(TestHelpers.CreateNullLogger<WorkspaceLoader>());
        var invalidPath = "C:/nonexistent/project.csproj";

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => loader.LoadProjectAsync(invalidPath));
    }

    [Fact]
    public async Task LoadSolutionAsync_InvalidPath_ThrowsFileNotFoundException()
    {
        var loader = new WorkspaceLoader(TestHelpers.CreateNullLogger<WorkspaceLoader>());
        var invalidPath = "C:/nonexistent/solution.sln";

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => loader.LoadSolutionAsync(invalidPath));
    }

    [Fact]
    public void LoadSolutionAsync_ContainsExpectedProjects()
    {
        var projectNames = _fixture.TestDataSolution.Projects.Select(p => p.Name).ToList();
        Assert.Contains("WithUnreferencedCode", projectNames);
        Assert.Contains("WithoutUnreferencedCode", projectNames);
        Assert.Contains("MixedAccessibility", projectNames);
    }
}
