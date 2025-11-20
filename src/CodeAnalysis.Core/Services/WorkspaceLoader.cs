using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

namespace CodeAnalysis.Core.Services;

/// <summary>
/// Service for loading Roslyn workspaces from solution and project files
/// </summary>
public class WorkspaceLoader : IDisposable
{
    private readonly ILogger<WorkspaceLoader> _logger;
    private static bool _msbuildRegistered = false;
    private static readonly object _lock = new();
    private MSBuildWorkspace? _workspace;

    public WorkspaceLoader(ILogger<WorkspaceLoader> logger)
    {
        _logger = logger;
        EnsureMSBuildRegistered();
    }

    private void EnsureMSBuildRegistered()
    {
        lock (_lock)
        {
            if (!_msbuildRegistered)
            {
                try
                {
                    // Register MSBuild defaults - this must be done before creating MSBuildWorkspace
                    MSBuildLocator.RegisterDefaults();
                    _msbuildRegistered = true;
                    _logger.LogInformation("MSBuild registered successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to register MSBuild");
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Loads a solution file and returns the workspace
    /// </summary>
    public async Task<Solution> LoadSolutionAsync(string solutionPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(solutionPath))
        {
            throw new FileNotFoundException($"Solution file not found: {solutionPath}");
        }

        _logger.LogInformation("Loading solution: {SolutionPath}", solutionPath);

        _workspace = MSBuildWorkspace.Create();

        // Log workspace failures
        _workspace.WorkspaceFailed += (sender, args) =>
        {
            if (args.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
            {
                _logger.LogWarning("Workspace failure: {Message}", args.Diagnostic.Message);
            }
        };

        try
        {
            var solution = await _workspace.OpenSolutionAsync(solutionPath, cancellationToken: cancellationToken);
            _logger.LogInformation("Solution loaded successfully with {ProjectCount} projects", solution.ProjectIds.Count);
            return solution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load solution: {SolutionPath}", solutionPath);
            throw;
        }
    }

    /// <summary>
    /// Loads a project file and returns the workspace
    /// </summary>
    public async Task<Project> LoadProjectAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(projectPath))
        {
            throw new FileNotFoundException($"Project file not found: {projectPath}");
        }

        _logger.LogInformation("Loading project: {ProjectPath}", projectPath);

        _workspace = MSBuildWorkspace.Create();

        // Log workspace failures
        _workspace.WorkspaceFailed += (sender, args) =>
        {
            if (args.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
            {
                _logger.LogWarning("Workspace failure: {Message}", args.Diagnostic.Message);
            }
        };

        try
        {
            var project = await _workspace.OpenProjectAsync(projectPath, cancellationToken: cancellationToken);
            _logger.LogInformation("Project loaded successfully: {ProjectName}", project.Name);
            return project;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load project: {ProjectPath}", projectPath);
            throw;
        }
    }

    public void Dispose()
    {
        _workspace?.Dispose();
    }
}
