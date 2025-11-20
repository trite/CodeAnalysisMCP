namespace CodeAnalysis.Core.Models;

/// <summary>
/// Options for configuring code analysis behavior
/// </summary>
public class AnalysisOptions
{
    /// <summary>
    /// Whether to include public members in unreferenced code detection.
    /// Default is true - public members will be flagged if unreferenced.
    /// </summary>
    public bool IncludePublicMembers { get; set; } = true;

    /// <summary>
    /// Whether to include entry points in unreferenced code detection.
    /// Entry points include Main methods, test methods, controllers, etc.
    /// Default is false - entry points will be excluded from results.
    /// </summary>
    public bool IncludeEntryPoints { get; set; } = false;

    /// <summary>
    /// Optional filter to specific projects by name (case-insensitive substring match).
    /// If null or empty, all projects are analyzed.
    /// </summary>
    public List<string>? ProjectFilter { get; set; }

    /// <summary>
    /// Optional filter to specific files by path (case-insensitive substring match).
    /// If null or empty, all files are analyzed.
    /// </summary>
    public List<string>? FileFilter { get; set; }
}
