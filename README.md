# CodeAnalysisMCP

A Model Context Protocol (MCP) server that provides code analysis capabilities for .NET C# projects and monorepos using Roslyn. This service enables AI agents to detect unreferenced code, analyze solution structures, and perform other code quality checks.

## Features

- **Unreferenced Code Detection**: Find classes, methods, properties, and fields that are never referenced in your codebase
- **Configurable Scope**: Analyze entire solutions, specific projects, or filter by file paths
- **Entry Point Awareness**: Optionally exclude entry points (Main methods, test methods, controllers) from analysis
- **Public Member Filtering**: Choose whether to include or exclude public members from analysis
- **Detailed Results**: Get comprehensive reports with file paths, line numbers, member types, and accessibility levels

## Available Tools

### 1. AnalyzeSolution

Analyzes a .NET solution file (.sln) for unreferenced code members.

**Parameters:**

- `solutionPath` (string, required): Full path to the .sln file
- `includePublicMembers` (bool, optional, default: true): Include public members in results
- `includeEntryPoints` (bool, optional, default: false): Include entry points in results
- `projectFilter` (string, optional): Comma-separated list of project name filters
- `fileFilter` (string, optional): Comma-separated list of file path filters

### 2. AnalyzeProject

Analyzes a single .NET project file (.csproj) for unreferenced code members.

**Parameters:**

- `projectPath` (string, required): Full path to the .csproj file
- `includePublicMembers` (bool, optional, default: true): Include public members in results
- `includeEntryPoints` (bool, optional, default: false): Include entry points in results
- `fileFilter` (string, optional): Comma-separated list of file path filters

### 3. GetSolutionInfo

Gets summary information about projects in a .NET solution.

**Parameters:**

- `solutionPath` (string, required): Full path to the .sln file

## Requirements

- .NET 9.0 or later
- MSBuild (included with Visual Studio or .NET SDK)

## Building

```bash
dotnet build
```

## Running as MCP Server

The server communicates via stdio and is designed to be used with MCP-compatible AI clients.

```bash
dotnet run
```

## Configuration for Claude Desktop

Add to your Claude Desktop configuration file:

**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "code-analysis": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\path\\to\\CodeAnalysisMCP\\CodeAnalysisMCP.csproj"
      ]
    }
  }
}
```

## Example Usage

Once configured, you can ask Claude to analyze your code:

- "Analyze the solution at C:\MyProject\MyProject.sln and find all unreferenced code"
- "Check the project C:\MyProject\MyLib\MyLib.csproj for unused private methods"
- "Find unreferenced code in MyProject.sln but exclude test projects"
- "What projects are in the solution at C:\MyProject\MyProject.sln?"

## How It Works

1. **Workspace Loading**: Uses Roslyn's `MSBuildWorkspace` to load solutions and projects
2. **Symbol Analysis**: Traverses the syntax tree to find all declared symbols
3. **Reference Finding**: Uses `SymbolFinder.FindReferencesAsync` to find all references to each symbol
4. **Filtering**: Applies configurable filters for public members, entry points, projects, and files
5. **Results**: Returns detailed JSON with member information and statistics

## Architecture

- **Models**: Data transfer objects for analysis options and results
- **Services**:
  - `WorkspaceLoader`: Handles loading of solutions and projects via Roslyn
  - `UnreferencedCodeAnalyzer`: Performs the actual code analysis
- **Tools**: MCP tool definitions that expose analysis capabilities to AI agents

## Limitations

- Only analyzes C# code (via Roslyn)
- Requires projects to be buildable (MSBuild must be able to restore and process them)
- Analysis can be slow for very large solutions (consider using project or file filters)
- Does not detect unreachable code (code after return statements, etc.) - only unreferenced symbols

## Future Enhancements

Potential features for future releases:

- Dead code detection (unreachable code)
- Cyclomatic complexity analysis
- Code duplication detection
- Dependency graph analysis
- Cross-reference reports

## License

See LICENSE.md for details.
