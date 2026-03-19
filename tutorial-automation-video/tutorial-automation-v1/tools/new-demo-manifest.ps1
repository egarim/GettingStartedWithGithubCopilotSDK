[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$RepoRoot,
    [string]$OutputPath = (Join-Path $RepoRoot 'tutorial-manifest.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = (Resolve-Path $RepoRoot).Path
$demos = foreach ($dir in Get-ChildItem -Path $RepoRoot -Directory | Sort-Object Name) {
    $stepsDir = Join-Path $dir.FullName 'steps'
    $csproj = Get-ChildItem -Path $dir.FullName -Filter '*.csproj' -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $csproj) { continue }

    $steps = @()
    if (Test-Path $stepsDir) {
        $steps = Get-ChildItem -Path $stepsDir -Filter 'step*.cs' | Sort-Object Name | ForEach-Object {
            [ordered]@{
                stepFile = $_.Name
                stepNumber = [int]([regex]::Match($_.BaseName, '\d+').Value)
                title = $_.BaseName
                runAfter = $false
                pauseMs = 2000
            }
        }
    }

    [ordered]@{
        demoName = $dir.Name
        projectFile = $csproj.Name
        hasSteps = $steps.Count -gt 0
        stepCount = $steps.Count
        scriptFile = if (Test-Path (Join-Path $dir.FullName 'DEMO_SCRIPT.txt')) { 'DEMO_SCRIPT.txt' } else { $null }
        steps = $steps
    }
}

[ordered]@{
    generatedAt = (Get-Date).ToString('s')
    repoRoot = $RepoRoot
    demos = @($demos)
} | ConvertTo-Json -Depth 8 | Set-Content $OutputPath -Encoding UTF8

Write-Host "Manifest written to $OutputPath" -ForegroundColor Green
