# Tutorial automation v1 for your C# demos

This package is a Windows-first replay pipeline designed around the structure already present in your ZIP.

It assumes:

- your repo contains one folder per demo
- each demo has a `steps/` folder with `step01.cs`, `step02.cs`, etc.
- you want to **show VS Code on screen**
- you record with **OBS**

## What is inside

- `tools/prepare-demo.ps1` — creates a clean replay workspace for one demo
- `tools/next-step.ps1` — advances the workspace to the next snapshot
- `tools/run-demo.ps1` — runs `dotnet run` in the workspace
- `tools/new-demo-manifest.ps1` — scans the repo and generates a manifest
- `tools/replay-demo.ahk` — hotkeys for VS Code playback on Windows
- `docs/IMPLEMENTATION_PLAN.md` — how to use and extend this
- `docs/OBS_VSCODE_SETUP.md` — recommended recording setup

## Recommended first prototype

Start with `01.ClientDemo`.

## Quick start

1. Unzip this package.
2. Install AutoHotkey v2 on Windows.
3. Make sure `code` and `dotnet` are available from PowerShell.
4. Run:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.	ools\prepare-demo.ps1 -RepoRoot "C:\path\to\GettingStartedWithGithubCopilitSDK" -Demo "01.ClientDemo" -OpenCode -Force
```

This creates a clean workspace at:

```text
%USERPROFILE%\TutorialReplay\01.ClientDemo
```

Then:

5. Edit `tools\replay-demo.ahk` and set `workspacePath` to your workspace.
6. Run the `.ahk` script.
7. Focus `Program.cs` in VS Code.
8. Use the hotkeys:

- `Win+Alt+N` → next step
- `Win+Alt+R` → run `dotnet run`
- `Win+Alt+S` → show current state
- `Win+Alt+B` → focus Explorer in VS Code

## How v1 behaves

v1 is intentionally simple:

- step files remain the source of truth
- every step replaces the current `Program.cs`
- git commits are created step-by-step inside the workspace
- VS Code is the visible editor layer
- OBS records the deterministic playback

This is already enough to remove most recording mistakes.

## What to improve in v2

- replace full-file swaps with diff-based block replay
- add per-step narration metadata
- add automatic terminal clearing between runs
- add chapter cards / captions with Remotion
