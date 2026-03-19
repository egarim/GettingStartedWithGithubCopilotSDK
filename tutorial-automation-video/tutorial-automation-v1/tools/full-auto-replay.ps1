<#
.SYNOPSIS
    Fully automated tutorial replay — no manual clicks needed.
    Prepares workspace, opens VS Code, starts OBS recording,
    steps through all steps with pauses, runs dotnet after each, stops recording.

    Run with: powershell -STA -ExecutionPolicy Bypass -File full-auto-replay.ps1 -Demo "01.ClientDemo"

.PARAMETER Demo
    Demo folder name (e.g. "01.ClientDemo")

.PARAMETER PauseBetweenSteps
    Seconds to pause between steps (default 8)

.PARAMETER PauseAfterRun
    Seconds to pause after dotnet run output (default 5)

.PARAMETER SkipPrepare
    Skip workspace preparation (use existing workspace)
#>
param(
    [Parameter(Mandatory)][string]$Demo,
    [int]$PauseBetweenSteps = 8,
    [int]$PauseAfterRun = 5,
    [switch]$SkipPrepare
)

$ErrorActionPreference = "Stop"
$repoRoot   = "C:\Users\joche\source\repos\GettingStartedWithGithubCopilitSDK"
$toolsDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$workspace  = "$env:USERPROFILE\TutorialReplay\$Demo"
$obsWsUrl   = "ws://127.0.0.1:4455"

Add-Type -AssemblyName System.Windows.Forms
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Win32 {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
}
"@

# ── Helper: Focus VS Code window ────────────────────────────────────────
function Focus-VsCode {
    $proc = Get-Process -Name "Code" -ErrorAction SilentlyContinue |
            Where-Object { $_.MainWindowHandle -ne [IntPtr]::Zero } |
            Select-Object -First 1
    if ($proc) {
        [Win32]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
        Start-Sleep -Milliseconds 500
        return $true
    }
    return $false
}

# ── Helper: Count lines in a file ────────────────────────────────────────
function Get-LineCount {
    param([string]$Path)
    return ([System.IO.File]::ReadAllLines($Path)).Count
}

# ── Helper: Slow scroll through file in VS Code ─────────────────────────
function Scroll-ThroughFile {
    param([int]$TotalLines, [int]$VisibleLines = 30, [int]$ScrollDelayMs = 400)

    # Go to top of file first: Ctrl+Home
    [System.Windows.Forms.SendKeys]::SendWait("^{HOME}")
    Start-Sleep -Milliseconds 500

    # Calculate how many page-downs needed
    $scrolls = [Math]::Ceiling(($TotalLines - $VisibleLines) / ($VisibleLines * 0.7))
    if ($scrolls -lt 1) { $scrolls = 0 }

    for ($i = 0; $i -lt $scrolls; $i++) {
        [System.Windows.Forms.SendKeys]::SendWait("{PGDN}")
        Start-Sleep -Milliseconds $ScrollDelayMs
    }

    # Return to top
    Start-Sleep -Milliseconds 300
    [System.Windows.Forms.SendKeys]::SendWait("^{HOME}")
    Start-Sleep -Milliseconds 300
}

# ── Helper: Send OBS WebSocket 5.x request ──────────────────────────────
function Send-ObsRequest {
    param([string]$RequestType, [hashtable]$RequestData = @{})

    $ws = [System.Net.WebSockets.ClientWebSocket]::new()
    $ct = [System.Threading.CancellationToken]::None

    try {
        $ws.ConnectAsync([Uri]$obsWsUrl, $ct).GetAwaiter().GetResult()

        $buf = [byte[]]::new(8192)
        $seg = [ArraySegment[byte]]::new($buf)

        # Read Hello
        $ws.ReceiveAsync($seg, $ct).GetAwaiter().GetResult() | Out-Null

        # Send Identify (no auth)
        $identify = @{ op = 1; d = @{ rpcVersion = 1 } } | ConvertTo-Json -Compress
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($identify)
        $ws.SendAsync([ArraySegment[byte]]::new($bytes), [System.Net.WebSockets.WebSocketMessageType]::Text, $true, $ct).GetAwaiter().GetResult()

        # Read Identified
        $ws.ReceiveAsync($seg, $ct).GetAwaiter().GetResult() | Out-Null

        # Send request
        $req = @{
            op = 6
            d  = @{
                requestType = $RequestType
                requestId   = [guid]::NewGuid().ToString()
                requestData = $RequestData
            }
        } | ConvertTo-Json -Depth 5 -Compress
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($req)
        $ws.SendAsync([ArraySegment[byte]]::new($bytes), [System.Net.WebSockets.WebSocketMessageType]::Text, $true, $ct).GetAwaiter().GetResult()

        # Read response
        $result = $ws.ReceiveAsync($seg, $ct).GetAwaiter().GetResult()
        $response = [System.Text.Encoding]::UTF8.GetString($buf, 0, $result.Count)
        return ($response | ConvertFrom-Json)
    }
    finally {
        if ($ws.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
            $ws.CloseAsync([System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure, "", $ct).GetAwaiter().GetResult()
        }
        $ws.Dispose()
    }
}

function Try-ObsRequest {
    param([string]$RequestType, [hashtable]$RequestData = @{}, [int]$Retries = 5)
    for ($i = 0; $i -lt $Retries; $i++) {
        try {
            return Send-ObsRequest -RequestType $RequestType -RequestData $RequestData
        } catch {
            if ($i -lt $Retries - 1) {
                Write-Host "  OBS not ready, retrying in 3s... ($($i+1)/$Retries)" -ForegroundColor Yellow
                Start-Sleep -Seconds 3
            } else { throw $_ }
        }
    }
}

# ── 1. Prepare workspace ────────────────────────────────────────────────
if (-not $SkipPrepare) {
    Write-Host "`n=== Preparing workspace for $Demo ===" -ForegroundColor Cyan
    & "$toolsDir\prepare-demo.ps1" -RepoRoot $repoRoot -Demo $Demo -OpenCode -Force
}

$state = Get-Content "$workspace\.tutorial\state.json" | ConvertFrom-Json
$totalSteps = $state.totalSteps
Write-Host "`nDemo: $Demo | Total steps: $totalSteps" -ForegroundColor Green

# ── 2. Wait for VS Code ─────────────────────────────────────────────────
Write-Host "`nWaiting for VS Code to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Open Program.cs and ensure it's the active tab
& code "$workspace\Program.cs" --reuse-window
Start-Sleep -Seconds 3
Focus-VsCode | Out-Null

# ── 3. Configure OBS scene with display capture ─────────────────────────
Write-Host "`n=== Configuring OBS ===" -ForegroundColor Cyan
try {
    $scenes = Try-ObsRequest -RequestType "GetSceneList"
    $currentScene = $scenes.d.responseData.currentProgramSceneName
    Write-Host "  Current OBS scene: $currentScene" -ForegroundColor Gray

    # Create scene if needed
    try {
        Try-ObsRequest -RequestType "CreateScene" -RequestData @{ sceneName = "Tutorial Recording" } | Out-Null
        Write-Host "  Created 'Tutorial Recording' scene" -ForegroundColor Green
    } catch {
        Write-Host "  Scene 'Tutorial Recording' already exists" -ForegroundColor Gray
    }

    # Switch to it
    Try-ObsRequest -RequestType "SetCurrentProgramScene" -RequestData @{ sceneName = "Tutorial Recording" } | Out-Null

    # Remove old broken window capture if present
    try {
        Try-ObsRequest -RequestType "RemoveInput" -RequestData @{ inputName = "VS Code Capture" } | Out-Null
    } catch {}

    # Add display/monitor capture (works reliably, no black screen)
    try {
        Try-ObsRequest -RequestType "CreateInput" -RequestData @{
            sceneName     = "Tutorial Recording"
            inputName     = "Display Capture"
            inputKind     = "monitor_capture"
            inputSettings = @{
                monitor    = 0
                capture_cursor = $true
            }
        } | Out-Null
        Write-Host "  Created display capture source" -ForegroundColor Green
    } catch {
        Write-Host "  Display capture source may already exist" -ForegroundColor Gray
    }

    Write-Host "  OBS configured with display capture!" -ForegroundColor Green
} catch {
    Write-Host "  WARNING: Could not configure OBS: $_" -ForegroundColor Red
    Write-Host "  Continuing anyway..." -ForegroundColor Yellow
}

# ── 4. Maximize VS Code (fills screen for display capture) ──────────────
Focus-VsCode | Out-Null
# Send Win+Up to maximize VS Code window
[System.Windows.Forms.SendKeys]::SendWait("^+p")
Start-Sleep -Milliseconds 500
[System.Windows.Forms.SendKeys]::SendWait("View: Toggle Full Screen{ENTER}")
Start-Sleep -Seconds 2

# ── 5. Start OBS recording ──────────────────────────────────────────────
Write-Host "`n=== Starting OBS recording ===" -ForegroundColor Cyan
try {
    Try-ObsRequest -RequestType "StartRecord" | Out-Null
    Write-Host "OBS recording started!" -ForegroundColor Green
} catch {
    Write-Host "WARNING: Could not start OBS recording: $_" -ForegroundColor Red
    Write-Host "Continuing without OBS..." -ForegroundColor Yellow
}

Start-Sleep -Seconds 2

# ── 6. Step through the tutorial ─────────────────────────────────────────
for ($step = 1; $step -le $totalSteps; $step++) {

    Write-Host "`n=== Step $step / $totalSteps ===" -ForegroundColor Cyan

    if ($step -gt 1) {
        # Advance to next step (writes Program.cs on disk via byte copy)
        & "$toolsDir\next-step.ps1" -Workspace $workspace
        Start-Sleep -Milliseconds 500

        # Focus VS Code editor (not terminal)
        Focus-VsCode | Out-Null

        # Revert file from disk: Ctrl+Shift+P > "Revert File"
        [System.Windows.Forms.SendKeys]::SendWait("^+p")
        Start-Sleep -Milliseconds 500
        [System.Windows.Forms.SendKeys]::SendWait("Revert File{ENTER}")
        Start-Sleep -Seconds 1

        Write-Host "  Code updated (file reverted from disk)" -ForegroundColor Gray
    }

    # Scroll through the file so viewer can see all the code
    Focus-VsCode | Out-Null
    $lineCount = Get-LineCount -Path "$workspace\Program.cs"
    Write-Host "  Scrolling through $lineCount lines..." -ForegroundColor Gray
    Scroll-ThroughFile -TotalLines $lineCount -ScrollDelayMs 400

    Write-Host "  Pausing $PauseBetweenSteps seconds..." -ForegroundColor Yellow
    Start-Sleep -Seconds $PauseBetweenSteps

    # Run dotnet run from PowerShell (waits for completion — no sync issues)
    Write-Host "  Running dotnet run..." -ForegroundColor Gray
    $runOutput = & dotnet run --project "$workspace" 2>&1
    $runText = ($runOutput | Out-String).Trim()
    Write-Host $runText

    # Show output briefly
    Write-Host "  Pausing $PauseAfterRun seconds after run..." -ForegroundColor Yellow
    Start-Sleep -Seconds $PauseAfterRun

    # Refocus VS Code editor
    Focus-VsCode | Out-Null
    [System.Windows.Forms.SendKeys]::SendWait("^1")
    Start-Sleep -Milliseconds 300
}

# ── 7. Exit full screen ─────────────────────────────────────────────────
Focus-VsCode | Out-Null
[System.Windows.Forms.SendKeys]::SendWait("^+p")
Start-Sleep -Milliseconds 500
[System.Windows.Forms.SendKeys]::SendWait("View: Toggle Full Screen{ENTER}")
Start-Sleep -Seconds 1

# ── 8. Stop OBS recording ───────────────────────────────────────────────
Write-Host "`n=== Stopping OBS recording ===" -ForegroundColor Cyan
try {
    $result = Try-ObsRequest -RequestType "StopRecord"
    $outputPath = $result.d.responseData.outputPath
    Write-Host "Recording saved to: $outputPath" -ForegroundColor Green
} catch {
    Write-Host "WARNING: Could not stop OBS recording: $_" -ForegroundColor Red
}

Write-Host "`n=== Done! $Demo fully recorded ($totalSteps steps) ===" -ForegroundColor Green
