#Requires AutoHotkey v2.0
#SingleInstance Force

; -----------------------------------------------------------------------------
; replay-demo.ahk
; Hotkeys for VS Code tutorial playback on Windows.
;
; BEFORE RUNNING:
;   1) Edit workspacePath below.
;   2) Open the workspace in VS Code.
;   3) Keep Program.cs focused when stepping.
;
; HOTKEYS:
;   Win+Alt+N  -> advance to next step and replace Program.cs content
;   Win+Alt+R  -> run `dotnet run` in the integrated terminal
;   Win+Alt+S  -> show current state
;   Win+Alt+B  -> reveal explorer / keep VS Code focused
; -----------------------------------------------------------------------------

global workspacePath := "C:\Users\joche\TutorialReplay\01.ClientDemo"
global powershellExe := "powershell.exe"

actionDelay := 120

RunPowerShell(args) {
    global powershellExe
    cmd := Format('{} -NoProfile -ExecutionPolicy Bypass -File {}', powershellExe, args)
    RunWait(cmd,, "Hide")
}

ReadState() {
    global workspacePath
    stateFile := workspacePath "\.tutorial\state.json"
    if !FileExist(stateFile) {
        return "State file not found."
    }
    return FileRead(stateFile, "UTF-8")
}

EnsureVsCode() {
    if !WinExist("ahk_exe Code.exe") {
        MsgBox "VS Code window not found. Open the workspace first."
        return false
    }
    WinActivate "ahk_exe Code.exe"
    WinWaitActive "ahk_exe Code.exe",, 2
    return true
}

#!n:: {
    global workspacePath, actionDelay
    if !EnsureVsCode() {
        return
    }

    script := """" A_ScriptDir "\next-step.ps1" """ -Workspace """" workspacePath """"
    RunPowerShell(script)

    programPath := workspacePath "\Program.cs"
    if !FileExist(programPath) {
        MsgBox "Program.cs not found in workspace."
        return
    }

    clipSaved := A_Clipboard
    A_Clipboard := FileRead(programPath, "UTF-8")
    ClipWait 1

    Send "^a"
    Sleep actionDelay
    Send "{Backspace}"
    Sleep actionDelay
    Send "^v"
    Sleep actionDelay
    Send "^s"
    Sleep actionDelay

    A_Clipboard := clipSaved
}

#!r:: {
    if !EnsureVsCode() {
        return
    }
    Send "^`"
    Sleep 200
    Send "dotnet run{Enter}"
}

#!s:: {
    MsgBox ReadState(), "Tutorial Replay State"
}

#!b:: {
    if !EnsureVsCode() {
        return
    }
    Send "^+e"
}
