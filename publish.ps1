#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build and publish CodeAnalysis.Mcp for local testing or distribution.

.PARAMETER Target
    The publish target: 'tool' (NuGet tool), 'standalone' (self-contained exe), or 'all'

.PARAMETER Runtime
    Runtime identifier for standalone builds (default: win-x64)

.EXAMPLE
    ./publish.ps1 -Target tool
    ./publish.ps1 -Target standalone -Runtime linux-x64
    ./publish.ps1 -Target all
#>

param(
    [ValidateSet('tool', 'standalone', 'all')]
    [string]$Target = 'all',

    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'
$ProjectPath = "src/CodeAnalysis.Mcp/CodeAnalysis.Mcp.csproj"
$PublishDir = "./publish"

function Write-Step($message) {
    Write-Host "`n==> $message" -ForegroundColor Cyan
}

# Clean previous builds
Write-Step "Cleaning previous builds..."
if (Test-Path $PublishDir) {
    Remove-Item -Recurse -Force $PublishDir
}

# Run tests first
Write-Step "Running tests..."
dotnet test --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

# Build NuGet tool package
if ($Target -eq 'tool' -or $Target -eq 'all') {
    Write-Step "Building NuGet tool package..."
    dotnet pack $ProjectPath -c Release

    if ($LASTEXITCODE -eq 0) {
        $nupkgPath = Get-ChildItem "src/CodeAnalysis.Mcp/nupkg/*.nupkg" | Select-Object -First 1
        Write-Host "`nNuGet package created: $($nupkgPath.FullName)" -ForegroundColor Green

        Write-Host "`nTo install locally as a global tool:" -ForegroundColor Yellow
        Write-Host "  dotnet tool install --global --add-source src/CodeAnalysis.Mcp/nupkg CodeAnalysis.Mcp"
        Write-Host "`nTo update an existing installation:" -ForegroundColor Yellow
        Write-Host "  dotnet tool update --global --add-source src/CodeAnalysis.Mcp/nupkg CodeAnalysis.Mcp"
        Write-Host "`nThen configure in .mcp.json:" -ForegroundColor Yellow
        Write-Host '  { "mcpServers": { "code-analysis": { "command": "code-analysis-mcp", "args": [] } } }'
    }
}

# Build standalone executable
if ($Target -eq 'standalone' -or $Target -eq 'all') {
    Write-Step "Building standalone executable for $Runtime..."
    $outputDir = "$PublishDir/$Runtime"

    dotnet publish $ProjectPath -c Release -r $Runtime --self-contained -o $outputDir

    if ($LASTEXITCODE -eq 0) {
        $exePath = if ($Runtime -like 'win-*') {
            Get-ChildItem "$outputDir/*.exe" | Select-Object -First 1
        } else {
            Get-Item "$outputDir/CodeAnalysis.Mcp"
        }

        Write-Host "`nStandalone executable created: $($exePath.FullName)" -ForegroundColor Green
        Write-Host "`nConfigure in .mcp.json:" -ForegroundColor Yellow
        Write-Host "  { `"mcpServers`": { `"code-analysis`": { `"command`": `"$($exePath.FullName -replace '\\', '\\\\')`", `"args`": [] } } }"
    }
}

Write-Host "`nPublish complete!" -ForegroundColor Green
