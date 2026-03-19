# Implementation plan

## Goal

Turn your existing demo repository into a repeatable tutorial-production system without rewriting the demos.

## Key idea

Your current `steps/stepNN.cs` files are already the content engine. The missing piece is a replay layer.

The system works like this:

1. `prepare-demo.ps1` creates a clean workspace.
2. `Program.cs` starts from `step01.cs`.
3. `next-step.ps1` advances to the next snapshot.
4. `replay-demo.ahk` visually replaces the file inside VS Code.
5. OBS records the playback.

## Why this is the right v1

- low risk
- easy to understand
- works with your existing course structure
- reproducible
- no dependence on Visual Studio UI automation

## Suggested production workflow

### Phase A — prep

```powershell
.\tools\prepare-demo.ps1 -RepoRoot "C:\repo\GettingStartedWithGithubCopilitSDK" -Demo "01.ClientDemo" -OpenCode -Force
```

### Phase B — recording

1. Open OBS.
2. Open the prepared VS Code workspace.
3. Start the AutoHotkey replay script.
4. Begin recording.
5. Use `Win+Alt+N` to advance.
6. Use `Win+Alt+R` whenever you want terminal output.

### Phase C — repeatability

If anything goes wrong:

- stop recording
- rerun `prepare-demo.ps1`
- start again from a clean workspace

## Notes about the current implementation

### What it supports well

- console demos
- snapshot-based code progression
- package restore per demo
- one-file `Program.cs` presentations

### What needs extra handling later

- `.csproj` visual edits on screen
- multi-file demos
- Blazor/UI demos
- diff-only block insertion instead of full-file replacement

## Recommended v2 direction

Add a `tutorial.json` file per demo with fields like:

```json
{
  "title": "01.ClientDemo",
  "steps": [
    {
      "step": 2,
      "title": "Create client",
      "runAfter": false,
      "pauseMs": 2500,
      "showFile": "Program.cs"
    }
  ]
}
```

Then update the replay engine to:

- read metadata
- insert only the changed block
- control pauses automatically
- show file-specific overlays later in Remotion
