using CodeAnalysis.Tests.Fixtures;
using Xunit;

namespace CodeAnalysis.Tests;

public class WithUnreferencedCodeTests : IClassFixture<WithUnreferencedCodeFixture>
{
    private readonly WithUnreferencedCodeFixture _fixture;

    public WithUnreferencedCodeTests(WithUnreferencedCodeFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void FindsUnreferencedMembers()
    {
        Assert.True(_fixture.Result.TotalCount > 0, "Should find unreferenced members");
    }

    [Fact]
    public void FindsUnusedClass()
    {
        Assert.Contains(_fixture.Result.UnreferencedMembers, m => m.MemberName.Contains("UnusedClass"));
    }

    [Fact]
    public void FindsUnusedMethod()
    {
        Assert.Contains(_fixture.Result.UnreferencedMembers, m => m.MemberName.Contains("UnusedMethod"));
    }

    [Fact]
    public void FindsUnusedProperty()
    {
        Assert.Contains(_fixture.Result.UnreferencedMembers, m => m.MemberName.Contains("UnusedProperty"));
    }

    [Fact]
    public void CountsByType_AreCorrect()
    {
        Assert.NotEmpty(_fixture.Result.CountByType);
        Assert.Equal(_fixture.Result.TotalCount, _fixture.Result.CountByType.Values.Sum());
    }
}

public class WithoutUnreferencedCodeTests : IClassFixture<WithoutUnreferencedCodeFixture>
{
    private readonly WithoutUnreferencedCodeFixture _fixture;

    public WithoutUnreferencedCodeTests(WithoutUnreferencedCodeFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void CountSummaries_MatchTotal()
    {
        var result = _fixture.ResultWithEntryPoints;
        Assert.Equal(result.TotalCount, result.CountByType.Values.Sum());
        Assert.Equal(result.TotalCount, result.CountByAccessibility.Values.Sum());
    }

    [Fact]
    public void AllMembers_HaveValidMetadata()
    {
        foreach (var member in _fixture.ResultWithEntryPoints.UnreferencedMembers)
        {
            Assert.False(string.IsNullOrEmpty(member.MemberName));
            Assert.False(string.IsNullOrEmpty(member.FilePath));
            Assert.True(member.LineNumber > 0);
        }
    }

    [Fact]
    public void ExcludesEntryPoints_WhenOptionSet()
    {
        Assert.DoesNotContain(_fixture.ResultWithoutEntryPoints.UnreferencedMembers, m =>
            m.MemberName.Contains("Main") && m.MemberType == "Method");
    }
}

public class MixedAccessibilityTests : IClassFixture<MixedAccessibilityFixture>
{
    private readonly MixedAccessibilityFixture _fixture;

    public MixedAccessibilityTests(MixedAccessibilityFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ExcludesPublicMembers_WhenOptionSet()
    {
        var publicMembers = _fixture.ResultWithoutPublic.UnreferencedMembers
            .Where(m => m.Accessibility == "Public")
            .ToList();

        Assert.Empty(publicMembers);
    }

    [Fact]
    public void FindsPrivateUnreferencedMembers()
    {
        Assert.Contains(_fixture.ResultWithPublic.UnreferencedMembers, m =>
            m.MemberName.Contains("_privateUnused") && m.Accessibility == "Private");
    }

    [Fact]
    public void CountsByAccessibility_AreCorrect()
    {
        Assert.NotEmpty(_fixture.ResultWithPublic.CountByAccessibility);
        Assert.Equal(_fixture.ResultWithPublic.TotalCount, _fixture.ResultWithPublic.CountByAccessibility.Values.Sum());
    }
}
