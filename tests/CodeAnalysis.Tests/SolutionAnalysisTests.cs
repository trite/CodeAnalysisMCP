using CodeAnalysis.Tests.Fixtures;
using Xunit;

namespace CodeAnalysis.Tests;

public class SolutionAnalysisTests : IClassFixture<TestDataSolutionFixture>
{
    private readonly TestDataSolutionFixture _fixture;

    public SolutionAnalysisTests(TestDataSolutionFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void LoadsAllProjects()
    {
        Assert.True(_fixture.FullResult.CountByProject.Count == 3, "Should have data for exactly 3 projects");
    }

    [Fact]
    public void WithProjectFilter_OnlyAnalyzesMatchingProjects()
    {
        Assert.Single(_fixture.FilteredResult.CountByProject);
        Assert.Contains("WithUnreferencedCode", _fixture.FilteredResult.CountByProject.Keys.First());
    }

    [Fact]
    public void AggregatesResultsFromAllProjects()
    {
        var result = _fixture.FullResult;
        Assert.Equal(result.TotalCount, result.UnreferencedMembers.Count);
        Assert.Equal(result.TotalCount, result.CountByType.Values.Sum());
        Assert.Equal(result.TotalCount, result.CountByAccessibility.Values.Sum());
        Assert.Equal(result.TotalCount, result.CountByProject.Values.Sum());
    }

    [Fact]
    public void IncludesFilePathAndLineNumber()
    {
        foreach (var member in _fixture.FullResult.UnreferencedMembers)
        {
            Assert.False(string.IsNullOrEmpty(member.FilePath), $"Member {member.MemberName} should have a file path");
            Assert.True(member.LineNumber > 0, $"Member {member.MemberName} should have a positive line number");
            Assert.False(string.IsNullOrEmpty(member.ProjectName), $"Member {member.MemberName} should have a project name");
        }
    }
}
