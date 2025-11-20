using System.ComponentModel;
using System.Text.Json;
using CodeAnalysis.Core.Models;
using CodeAnalysis.Core.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace CodeAnalysis.Mcp.Tools;

/// <summary>
/// MCP tools for code analysis
/// </summary>
[McpServerToolType]
public class CodeAnalysisTools
{
    private readonly UnreferencedCodeAnalyzer _analyzer;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<CodeAnalysisTools> _logger;

    public CodeAnalysisTools(
        UnreferencedCodeAnalyzer analyzer,
        ILoggerFactory loggerFactory,
        ILogger<CodeAnalysisTools> logger)
    {
        _analyzer = analyzer;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Analyzes a .NET solution file (.sln) for unreferenced code members like classes, methods, properties, and fields. Returns counts and locations of unreferenced code.")]
    public async Task<string> AnalyzeSolution(
        [Description("Full path to the .sln file to analyze")]
        string solutionPath,
        [Description("Include public members in results (default: true)")]
        bool includePublicMembers = true,
        [Description("Include entry points like Main, test methods, controllers in results (default: false)")]
        bool includeEntryPoints = false,
        [Description("Optional comma-separated list of project name filters (case-insensitive)")]
        string? projectFilter = null,
        [Description("Optional comma-separated list of file path filters (case-insensitive)")]
        string? fileFilter = null)
    {
        try
        {
            _logger.LogInformation("Analyzing solution: {SolutionPath}", solutionPath);

            var options = new AnalysisOptions
            {
                IncludePublicMembers = includePublicMembers,
                IncludeEntryPoints = includeEntryPoints,
                ProjectFilter = string.IsNullOrWhiteSpace(projectFilter)
                    ? null
                    : projectFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                FileFilter = string.IsNullOrWhiteSpace(fileFilter)
                    ? null
                    : fileFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            };

            using var loader = new WorkspaceLoader(_loggerFactory.CreateLogger<WorkspaceLoader>());
            var solution = await loader.LoadSolutionAsync(solutionPath);
            var result = await _analyzer.AnalyzeSolutionAsync(solution, options);

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing solution");
            return JsonSerializer.Serialize(new
            {
                error = ex.Message,
                stackTrace = ex.StackTrace
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool]
    [Description("Analyzes a single .NET project file (.csproj) for unreferenced code members. Returns counts and locations of unreferenced code.")]
    public async Task<string> AnalyzeProject(
        [Description("Full path to the .csproj file to analyze")]
        string projectPath,
        [Description("Include public members in results (default: true)")]
        bool includePublicMembers = true,
        [Description("Include entry points like Main, test methods, controllers in results (default: false)")]
        bool includeEntryPoints = false,
        [Description("Optional comma-separated list of file path filters (case-insensitive)")]
        string? fileFilter = null)
    {
        try
        {
            _logger.LogInformation("Analyzing project: {ProjectPath}", projectPath);

            var options = new AnalysisOptions
            {
                IncludePublicMembers = includePublicMembers,
                IncludeEntryPoints = includeEntryPoints,
                FileFilter = string.IsNullOrWhiteSpace(fileFilter)
                    ? null
                    : fileFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            };

            using var loader = new WorkspaceLoader(_loggerFactory.CreateLogger<WorkspaceLoader>());
            var project = await loader.LoadProjectAsync(projectPath);
            var result = await _analyzer.AnalyzeProjectAsync(project, options);

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing project");
            return JsonSerializer.Serialize(new
            {
                error = ex.Message,
                stackTrace = ex.StackTrace
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool]
    [Description("Gets summary information about projects in a .NET solution, including project names, paths, and document counts.")]
    public async Task<string> GetSolutionInfo(
        [Description("Full path to the .sln file")]
        string solutionPath)
    {
        try
        {
            _logger.LogInformation("Getting solution info: {SolutionPath}", solutionPath);

            using var loader = new WorkspaceLoader(_loggerFactory.CreateLogger<WorkspaceLoader>());
            var solution = await loader.LoadSolutionAsync(solutionPath);

            var projectInfo = solution.Projects.Select(p => new
            {
                p.Name,
                p.FilePath,
                p.Language,
                DocumentCount = p.Documents.Count(),
                p.OutputFilePath,
                p.AssemblyName
            }).ToList();

            var info = new
            {
                SolutionPath = solutionPath,
                ProjectCount = solution.ProjectIds.Count,
                Projects = projectInfo
            };

            return JsonSerializer.Serialize(info, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting solution info");
            return JsonSerializer.Serialize(new
            {
                error = ex.Message,
                stackTrace = ex.StackTrace
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
