namespace CodeAnalysis.Core.Models;

/// <summary>
/// Result of a code analysis operation
/// </summary>
public class AnalysisResult
{
    /// <summary>
    /// List of unreferenced members found
    /// </summary>
    public List<UnreferencedMemberInfo> UnreferencedMembers { get; set; } = new();

    /// <summary>
    /// Total count of unreferenced members
    /// </summary>
    public int TotalCount => UnreferencedMembers.Count;

    /// <summary>
    /// Count by member type
    /// </summary>
    public Dictionary<string, int> CountByType { get; set; } = new();

    /// <summary>
    /// Count by project
    /// </summary>
    public Dictionary<string, int> CountByProject { get; set; } = new();

    /// <summary>
    /// Count by accessibility
    /// </summary>
    public Dictionary<string, int> CountByAccessibility { get; set; } = new();

    /// <summary>
    /// Any errors or warnings encountered during analysis
    /// </summary>
    public List<string> Diagnostics { get; set; } = new();
}
