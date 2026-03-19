# OBS + VS Code setup

## VS Code

Use a clean, readable layout:

- font size: 28
- line height: 1.5
- minimap: off
- zoom so 30–45 lines are visible
- Explorer visible only when useful
- integrated terminal docked at the bottom
- dark theme with good contrast

The prep script already writes a `.vscode/settings.json` with tutorial-friendly defaults.

## OBS

Use one stable scene.

### Scene recommendation

- Source 1: display capture or window capture for VS Code
- Source 2: optional microphone input
- Source 3: optional webcam, but skip it for v1

### Recording recommendations

- 1920x1080 canvas
- 1920x1080 output
- 30 fps is enough for code tutorials
- keep the same VS Code window size every time

## Recording rhythm

Use this pattern repeatedly:

1. talk
2. `Win+Alt+N`
3. short pause
4. explain the new code
5. `Win+Alt+R` when you want output
6. continue

## Practical tip

Clear the terminal between runs when needed so each command looks intentional.

A simple manual way is:

```powershell
cls
```

You can add that to the AHK flow in v2 if you want a cleaner look.
