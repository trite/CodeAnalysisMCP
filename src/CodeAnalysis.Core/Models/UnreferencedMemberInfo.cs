namespace CodeAnalysis.Core.Models;

/// <summary>
/// Represents information about an unreferenced code member
/// </summary>
public class UnreferencedMemberInfo
{
    /// <summary>
    /// The fully qualified name of the member (e.g., "MyNamespace.MyClass.MyMethod")
    /// </summary>
    public required string MemberName { get; set; }

    /// <summary>
    /// The type of member (Class, Method, Property, Field, etc.)
    /// </summary>
    public required string MemberType { get; set; }

    /// <summary>
    /// The accessibility level (Public, Internal, Private, etc.)
    /// </summary>
    public required string Accessibility { get; set; }

    /// <summary>
    /// The file path where the member is declared
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// The line number where the member is declared
    /// </summary>
    public required int LineNumber { get; set; }

    /// <summary>
    /// The project name containing this member
    /// </summary>
    public required string ProjectName { get; set; }
}
