# Automated C# Tutorial Recording Pipeline

### VS Code + PowerShell + OBS + Step Snapshots

Author: Jose "Joche" Ojeda\
Purpose: Automatically replay C# tutorial steps inside VS Code for video
recording.

------------------------------------------------------------------------

# 1. Goal

Create a deterministic tutorial system where:

1.  Each tutorial has **step snapshots**
2.  A script prepares a **clean workspace**
3.  VS Code shows the editor
4.  Scripts advance through steps
5.  OBS records the session

This eliminates:

-   recording mistakes
-   copy‑paste errors
-   inconsistent demos
-   editing problems in videos

------------------------------------------------------------------------

# 2. Required Software

## VS Code

https://code.visualstudio.com/

Verify installation:

    code --version

------------------------------------------------------------------------

## .NET SDK

Install latest .NET SDK.

Verify:

    dotnet --version

------------------------------------------------------------------------

## OBS Studio

Used for recording.

https://obsproject.com/

------------------------------------------------------------------------

## PowerShell

Included with Windows.

------------------------------------------------------------------------

## AutoHotkey (optional)

For hotkey automation.

https://www.autohotkey.com/

------------------------------------------------------------------------

# 3. Repository Structure

Your repo example:

    C:\Users\joche\source\repos\GettingStartedWithGithubCopilitSDK

Example project layout:

    GettingStartedWithGithubCopilitSDK
       01.ClientDemo
           01.ClientDemo.csproj
           Program.cs
           steps
               step01.cs
               step02.cs
               step03.cs

Important concept:

    steps/

Contains snapshots of the program at each tutorial step.

------------------------------------------------------------------------

# 4. Automation Folder

Create:

    C:\Users\joche\tutorial-automation

Structure:

    tutorial-automation
        tools
            prepare-demo.ps1
            next-step.ps1
            run-demo.ps1
            replay-demo.ahk

------------------------------------------------------------------------

# 5. Demo Workspace

Temporary workspace:

    C:\Users\joche\tutorial-workspaces

Example generated workspace:

    tutorial-workspaces
        01.ClientDemo
            Program.cs
            01.ClientDemo.csproj

This ensures:

-   clean environment
-   repeatable tutorials
-   original repo remains untouched

------------------------------------------------------------------------

# 6. prepare-demo.ps1

Purpose: prepare a clean workspace and open VS Code.

Example usage:

``` powershell
.\tools\prepare-demo.ps1 `
-RepoRoot "C:\Users\joche\source\repos\GettingStartedWithGithubCopilitSDK" `
-Demo "01.ClientDemo" `
-OpenCode `
-Force
```

Actions:

1.  Deletes previous workspace
2.  Copies project files
3.  Copies `step01.cs` → `Program.cs`
4.  Runs `dotnet restore`
5.  Opens VS Code

------------------------------------------------------------------------

# 7. next-step.ps1

Advance the tutorial to the next step.

Example:

``` powershell
.\tools\next-step.ps1 -Demo "01.ClientDemo"
```

This replaces:

    Program.cs

with the next step snapshot.

------------------------------------------------------------------------

# 8. run-demo.ps1

Runs the application.

Example:

``` powershell
.\tools\run-demo.ps1 -Demo "01.ClientDemo"
```

Equivalent to:

    dotnet run

------------------------------------------------------------------------

# 9. AutoHotkey Automation (Optional)

Example hotkeys:

    Win + Alt + N → next tutorial step
    Win + Alt + R → run demo

Inside the script configure:

    workspacePath := "C:\Users\joche\tutorial-workspaces\01.ClientDemo"

Run by double‑clicking the `.ahk` file.

------------------------------------------------------------------------

# 10. Recording Workflow

Typical session:

1.  Start OBS
2.  Run prepare-demo
3.  Explain code
4.  Run next-step
5.  Explain change
6.  Run demo
7.  Repeat

------------------------------------------------------------------------

# 11. VS Code Recording Setup

Recommended:

Font Size

    22

Zoom Level

    1.2

Theme

    Dark+

Editor:

    Minimap off
    Word wrap off
    Line height 1.4

Terminal:

    bottom panel

------------------------------------------------------------------------

# 12. OBS Setup

Scene name:

    Coding Tutorial

Sources:

    Window Capture → VS Code

Resolution:

    1920x1080

FPS:

    30

Optional:

-   microphone
-   webcam overlay

------------------------------------------------------------------------

# 13. Why This Works

Advantages:

-   deterministic
-   repeatable tutorials
-   faster recording
-   fewer editing mistakes
-   consistent demonstrations

------------------------------------------------------------------------

# 14. Future Improvements

Possible upgrades:

### Diff Based Replay

Show only changed lines instead of replacing entire file.

### Step Metadata

Example:

``` json
{
  "step": 3,
  "title": "Add PingAsync",
  "runAfter": true,
  "pause": 3000
}
```

### AI Narration

Generate subtitles automatically.

### Remotion Integration

Generate:

-   intros
-   captions
-   code highlights
-   social media clips

------------------------------------------------------------------------

# 15. Recommended Workflow

    prepare-demo
    start OBS
    next-step
    explain
    run-demo
    repeat

------------------------------------------------------------------------

# 16. Long Term Vision

The system becomes a tutorial compiler.

Input:

    steps/
    tutorial.json
    narration.md

Output:

    recorded video
    captions
    short clips
    social media versions

------------------------------------------------------------------------

# 17. Example Final Command

    record-demo 01.ClientDemo

Everything launches automatically.
