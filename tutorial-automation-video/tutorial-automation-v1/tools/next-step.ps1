[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$Workspace,
    [switch]$EmitOnly,
    [switch]$NoGitCommit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Workspace = (Resolve-Path $Workspace).Path
$statePath = Join-Path $Workspace '.tutorial\state.json'
if (-not (Test-Path $statePath)) {
    throw "State file not found: $statePath"
}

$state = Get-Content $statePath -Raw | ConvertFrom-Json
$current = [int]$state.currentStep
$total = [int]$state.totalSteps
if ($current -ge $total) {
    Write-Host "All steps already applied ($current/$total)." -ForegroundColor Yellow
    exit 0
}

$nextNumber = $current + 1
$stepName = $state.steps[$nextNumber - 1]
$nextStepPath = Join-Path (Join-Path $state.demoPath 'steps') $stepName
if (-not (Test-Path $nextStepPath)) {
    throw "Next step file not found: $nextStepPath"
}

$programPath = Join-Path $Workspace 'Program.cs'
$before = if (Test-Path $programPath) { Get-Content $programPath -Raw } else { '' }
$after = Get-Content $nextStepPath -Raw

$tutorialPath = Join-Path $Workspace '.tutorial'
$after | Set-Content (Join-Path $tutorialPath 'next-step.txt') -Encoding UTF8
$before | Set-Content (Join-Path $tutorialPath 'current-step.txt') -Encoding UTF8

$payload = [ordered]@{
    currentStep = $current
    nextStep = $nextNumber
    totalSteps = $total
    stepFile = $stepName
    hasChanges = ($before -ne $after)
    generatedAt = (Get-Date).ToString('s')
}
$payload | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $tutorialPath 'pending-step.json') -Encoding UTF8

if ($EmitOnly) {
    Write-Host "Prepared step $nextNumber/$total -> $stepName"
    exit 0
}

# Copy bytes directly to preserve encoding (avoids BOM / character mangling)
[System.IO.File]::Copy($nextStepPath, $programPath, $true)
$state.currentStep = $nextNumber
$state | ConvertTo-Json -Depth 5 | Set-Content $statePath -Encoding UTF8

if (-not $NoGitCommit -and (Get-Command git -ErrorAction SilentlyContinue)) {
    Push-Location $Workspace
    try {
        git add Program.cs | Out-Null
        git commit -m ("step{0:d2}" -f $nextNumber) | Out-Null
    }
    finally {
        Pop-Location
    }
}

Write-Host "Applied step $nextNumber/$total -> $stepName" -ForegroundColor Green
