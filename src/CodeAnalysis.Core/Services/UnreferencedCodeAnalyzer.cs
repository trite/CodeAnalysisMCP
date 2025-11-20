using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.Extensions.Logging;
using CodeAnalysis.Core.Models;

namespace CodeAnalysis.Core.Services;

/// <summary>
/// Analyzes code for unreferenced members using Roslyn
/// </summary>
public class UnreferencedCodeAnalyzer
{
    private readonly ILogger<UnreferencedCodeAnalyzer> _logger;

    public UnreferencedCodeAnalyzer(ILogger<UnreferencedCodeAnalyzer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Analyzes a solution for unreferenced code members
    /// </summary>
    public async Task<AnalysisResult> AnalyzeSolutionAsync(
        Solution solution,
        AnalysisOptions options,
        CancellationToken cancellationToken = default)
    {
        var result = new AnalysisResult();
        var projectsToAnalyze = GetProjectsToAnalyze(solution, options);

        _logger.LogInformation("Analyzing {ProjectCount} projects for unreferenced code", projectsToAnalyze.Count);

        foreach (var project in projectsToAnalyze)
        {
            try
            {
                await AnalyzeProjectAsync(project, solution, options, result, cancellationToken);
            }
            catch (Exception ex)
            {
                var error = $"Error analyzing project {project.Name}: {ex.Message}";
                _logger.LogError(ex, error);
                result.Diagnostics.Add(error);
            }
        }

        // Calculate summaries
        result.CountByType = result.UnreferencedMembers
            .GroupBy(m => m.MemberType)
            .ToDictionary(g => g.Key, g => g.Count());

        result.CountByProject = result.UnreferencedMembers
            .GroupBy(m => m.ProjectName)
            .ToDictionary(g => g.Key, g => g.Count());

        result.CountByAccessibility = result.UnreferencedMembers
            .GroupBy(m => m.Accessibility)
            .ToDictionary(g => g.Key, g => g.Count());

        _logger.LogInformation("Analysis complete. Found {Count} unreferenced members", result.TotalCount);

        return result;
    }

    /// <summary>
    /// Analyzes a single project for unreferenced code members
    /// </summary>
    public async Task<AnalysisResult> AnalyzeProjectAsync(
        Project project,
        AnalysisOptions options,
        CancellationToken cancellationToken = default)
    {
        var result = new AnalysisResult();
        var solution = project.Solution;

        await AnalyzeProjectAsync(project, solution, options, result, cancellationToken);

        // Calculate summaries
        result.CountByType = result.UnreferencedMembers
            .GroupBy(m => m.MemberType)
            .ToDictionary(g => g.Key, g => g.Count());

        result.CountByProject = result.UnreferencedMembers
            .GroupBy(m => m.ProjectName)
            .ToDictionary(g => g.Key, g => g.Count());

        result.CountByAccessibility = result.UnreferencedMembers
            .GroupBy(m => m.Accessibility)
            .ToDictionary(g => g.Key, g => g.Count());

        return result;
    }

    private async Task AnalyzeProjectAsync(
        Project project,
        Solution solution,
        AnalysisOptions options,
        AnalysisResult result,
        CancellationToken cancellationToken)
    {
        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation == null)
        {
            result.Diagnostics.Add($"Could not get compilation for project {project.Name}");
            return;
        }

        _logger.LogDebug("Analyzing project: {ProjectName}", project.Name);

        var documentsToAnalyze = GetDocumentsToAnalyze(project, options);

        foreach (var document in documentsToAnalyze)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                await AnalyzeDocumentAsync(document, solution, options, result, cancellationToken);
            }
            catch (Exception ex)
            {
                var error = $"Error analyzing document {document.FilePath}: {ex.Message}";
                _logger.LogError(ex, error);
                result.Diagnostics.Add(error);
            }
        }
    }

    private async Task AnalyzeDocumentAsync(
        Document document,
        Solution solution,
        AnalysisOptions options,
        AnalysisResult result,
        CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        if (semanticModel == null)
            return;

        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root == null)
            return;

        var compilation = semanticModel.Compilation;

        // Get all declared symbols in this document
        var declaredSymbols = root.DescendantNodes()
            .Select(node => semanticModel.GetDeclaredSymbol(node, cancellationToken))
            .Where(symbol => symbol != null)
            .Cast<ISymbol>()
            .Where(symbol => ShouldAnalyzeSymbol(symbol, options))
            .ToList();

        foreach (var symbol in declaredSymbols)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            // Find all references to this symbol
            var references = await SymbolFinder.FindReferencesAsync(symbol, solution, cancellationToken);
            var referenceLocations = references
                .SelectMany(r => r.Locations)
                .Where(loc => !loc.IsImplicit) // Exclude implicit references
                .ToList();

            // A symbol is unreferenced if it only has references from its own declaration
            // (the declaration itself counts as a reference)
            var isUnreferenced = referenceLocations.Count <= 1;

            if (isUnreferenced)
            {
                var location = symbol.Locations.FirstOrDefault();
                if (location != null && location.IsInSource)
                {
                    var lineSpan = location.GetLineSpan();
                    result.UnreferencedMembers.Add(new UnreferencedMemberInfo
                    {
                        MemberName = symbol.ToDisplayString(),
                        MemberType = GetMemberType(symbol),
                        Accessibility = symbol.DeclaredAccessibility.ToString(),
                        FilePath = location.SourceTree?.FilePath ?? "Unknown",
                        LineNumber = lineSpan.StartLinePosition.Line + 1,
                        ProjectName = document.Project.Name
                    });
                }
            }
        }
    }

    private bool ShouldAnalyzeSymbol(ISymbol symbol, AnalysisOptions options)
    {
        // Skip compiler-generated symbols
        if (symbol.IsImplicitlyDeclared)
            return false;

        // Skip namespaces and modules
        if (symbol.Kind == SymbolKind.Namespace || symbol.Kind == SymbolKind.NetModule)
            return false;

        // Skip auto-generated properties' backing fields
        if (symbol is IFieldSymbol field && field.IsImplicitlyDeclared)
            return false;

        // Filter by accessibility
        if (!options.IncludePublicMembers && symbol.DeclaredAccessibility == Accessibility.Public)
            return false;

        // Filter by entry points
        if (!options.IncludeEntryPoints && IsEntryPoint(symbol))
            return false;

        return true;
    }

    private bool IsEntryPoint(ISymbol symbol)
    {
        // Check for Main method
        if (symbol is IMethodSymbol method)
        {
            if (method.Name == "Main" && method.IsStatic)
                return true;

            // Check for test method attributes
            var testAttributes = new[] { "Test", "TestMethod", "Fact", "Theory" };
            if (method.GetAttributes().Any(attr =>
                testAttributes.Any(ta => attr.AttributeClass?.Name.Contains(ta) == true)))
                return true;

            // Check for ASP.NET Core controller actions
            if (method.ContainingType?.BaseType?.Name.Contains("Controller") == true)
                return true;
        }

        // Check for types that might be entry points
        if (symbol is INamedTypeSymbol type)
        {
            // Check for controller classes
            if (type.Name.EndsWith("Controller") || type.BaseType?.Name.Contains("Controller") == true)
                return true;

            // Check for test classes
            var testClassAttributes = new[] { "TestClass", "TestFixture" };
            if (type.GetAttributes().Any(attr =>
                testClassAttributes.Any(ta => attr.AttributeClass?.Name.Contains(ta) == true)))
                return true;

            // Check for startup classes
            if (type.Name == "Startup" || type.Name == "Program")
                return true;
        }

        return false;
    }

    private string GetMemberType(ISymbol symbol)
    {
        return symbol.Kind switch
        {
            SymbolKind.NamedType => GetNamedTypeKind((INamedTypeSymbol)symbol),
            SymbolKind.Method => "Method",
            SymbolKind.Property => "Property",
            SymbolKind.Field => "Field",
            SymbolKind.Event => "Event",
            _ => symbol.Kind.ToString()
        };
    }

    private string GetNamedTypeKind(INamedTypeSymbol type)
    {
        return type.TypeKind switch
        {
            TypeKind.Class => "Class",
            TypeKind.Interface => "Interface",
            TypeKind.Struct => "Struct",
            TypeKind.Enum => "Enum",
            TypeKind.Delegate => "Delegate",
            _ => "Type"
        };
    }

    private List<Project> GetProjectsToAnalyze(Solution solution, AnalysisOptions options)
    {
        var projects = solution.Projects.ToList();

        if (options.ProjectFilter != null && options.ProjectFilter.Any())
        {
            projects = projects.Where(p =>
                options.ProjectFilter.Any(filter =>
                    p.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        return projects;
    }

    private List<Document> GetDocumentsToAnalyze(Project project, AnalysisOptions options)
    {
        var documents = project.Documents.ToList();

        if (options.FileFilter != null && options.FileFilter.Any())
        {
            documents = documents.Where(d =>
                options.FileFilter.Any(filter =>
                    d.FilePath?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();
        }

        return documents;
    }
}
