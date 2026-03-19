[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$RepoRoot,
    [Parameter(Mandatory=$true)][string]$Demo,
    [string]$WorkspaceRoot = "$HOME\TutorialReplay",
    [switch]$OpenCode,
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-DemoPath {
    param([string]$Root, [string]$Name)
    $candidate = Join-Path $Root $Name
    if (Test-Path $candidate) { return (Resolve-Path $candidate).Path }
    $matches = Get-ChildItem -Path $Root -Directory | Where-Object { $_.Name -like "*$Name*" }
    if ($matches.Count -eq 1) { return $matches[0].FullName }
    throw "Demo '$Name' not found under '$Root'."
}

$RepoRoot = (Resolve-Path $RepoRoot).Path
$DemoPath = Resolve-DemoPath -Root $RepoRoot -Name $Demo
$DemoName = Split-Path $DemoPath -Leaf
$StepsPath = Join-Path $DemoPath 'steps'
if (-not (Test-Path $StepsPath)) {
    throw "Demo '$DemoName' does not contain a steps folder."
}

$StepFiles = Get-ChildItem -Path $StepsPath -Filter 'step*.cs' | Sort-Object Name
if ($StepFiles.Count -eq 0) {
    throw "No step*.cs files found in '$StepsPath'."
}

$Csproj = Get-ChildItem -Path $DemoPath -Filter '*.csproj' | Select-Object -First 1
if (-not $Csproj) {
    throw "No .csproj file found in '$DemoPath'."
}

$Workspace = Join-Path $WorkspaceRoot $DemoName
if (Test-Path $Workspace) {
    if ($Force) {
        Remove-Item -Recurse -Force $Workspace
    }
    else {
        throw "Workspace '$Workspace' already exists. Use -Force to recreate it."
    }
}

New-Item -ItemType Directory -Path $Workspace -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $Workspace '.tutorial') -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $Workspace '.vscode') -Force | Out-Null

$ProjectName = [System.IO.Path]::GetFileNameWithoutExtension($Csproj.Name)
Write-Host "Creating workspace: $Workspace" -ForegroundColor Cyan

dotnet new console -n $ProjectName -o $Workspace --force | Out-Null
Copy-Item $Csproj.FullName (Join-Path $Workspace $Csproj.Name) -Force
Copy-Item $StepFiles[0].FullName (Join-Path $Workspace 'Program.cs') -Force

# Remove the generated csproj if dotnet new used another name.
Get-ChildItem $Workspace -Filter '*.csproj' |
    Where-Object { $_.Name -ne $Csproj.Name } |
    Remove-Item -Force

# Copy optional supporting assets used by richer demos.
foreach ($folder in 'Components','Data','Properties','Services','wwwroot') {
    $source = Join-Path $DemoPath $folder
    if (Test-Path $source) {
        Copy-Item $source (Join-Path $Workspace $folder) -Recurse -Force
    }
}
foreach ($file in 'appsettings.json','appsettings.Development.json','README.md','DEMO_SCRIPT.txt','Strings.cs') {
    $source = Join-Path $DemoPath $file
    if (Test-Path $source) {
        Copy-Item $source (Join-Path $Workspace $file) -Force
    }
}

$state = [ordered]@{
    repoRoot = $RepoRoot
    demoName = $DemoName
    demoPath = $DemoPath
    workspace = $Workspace
    projectName = $ProjectName
    currentStep = 1
    totalSteps = $StepFiles.Count
    csproj = $Csproj.Name
    steps = @($StepFiles | ForEach-Object { $_.Name })
    createdAt = (Get-Date).ToString('s')
}
$state | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $Workspace '.tutorial\state.json') -Encoding UTF8

$workspaceSettings = @'
{
  "editor.fontSize": 16,
  "editor.lineHeight": 1.5,
  "editor.minimap.enabled": false,
  "editor.scrollBeyondLastLine": false,
  "editor.wordWrap": "off",
  "editor.cursorBlinking": "solid",
  "editor.cursorSmoothCaretAnimation": "off",
  "editor.smoothScrolling": true,
  "editor.renderWhitespace": "none",
  "terminal.integrated.fontSize": 14,
  "terminal.integrated.defaultProfile.windows": "PowerShell",
  "workbench.startupEditor": "none",
  "files.autoSave": "off"
}
'@
$workspaceSettings | Set-Content (Join-Path $Workspace '.vscode\settings.json') -Encoding UTF8

$tasks = @'
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "Tutorial: Run Demo",
      "type": "shell",
      "command": "dotnet run",
      "group": "build",
      "problemMatcher": []
    }
  ]
}
'@
$tasks | Set-Content (Join-Path $Workspace '.vscode\tasks.json') -Encoding UTF8

Push-Location $Workspace
try {
    dotnet restore | Out-Null
    if (Get-Command git -ErrorAction SilentlyContinue) {
        git init | Out-Null
        git config user.name "Tutorial Replay" | Out-Null
        git config user.email "tutorial@local" | Out-Null
        git add . | Out-Null
        git commit -m "step01" | Out-Null
    }
}
finally {
    Pop-Location
}

if ($OpenCode) {
    $codeCmd = Get-Command code -ErrorAction SilentlyContinue
    if ($codeCmd) {
        & $codeCmd.Source $Workspace -g (Join-Path $Workspace 'Program.cs')
    }
    else {
        Write-Warning "VS Code command 'code' was not found in PATH. Open the workspace manually."
    }
}

Write-Host "Ready. Current step: 1/$($StepFiles.Count)" -ForegroundColor Green
Write-Host "Workspace: $Workspace"
