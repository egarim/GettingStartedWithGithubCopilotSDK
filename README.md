# GettingStartedWithGithubCopilitSDK

A hands-on course for the **GitHub Copilot SDK** (.NET). Each numbered folder is a self-contained demo that covers one SDK concept — from client lifecycle to tools, hooks, permissions, streaming, skills, MCP servers, custom agents, and BYOK models.

## Demos

| # | Folder | Topic |
|---|--------|-------|
| 01 | `01.ClientDemo` | Client lifecycle & connection |
| 02 | `02.SessionDemo` | Sessions, events & multi-turn |
| 03 | `03.ToolsDemo` | Custom tools (AIFunction) |
| 04 | `04.HooksDemo` | Pre/Post tool-use hooks |
| 05 | `05.PermissionsDemo` | Permission request handling |
| 06 | `06.AskUserDemo` | User input requests (ask_user) |
| 07 | `07.CompactionDemo` | Infinite sessions & compaction |
| 08 | `08.SkillsDemo` | Skill loading & configuration |
| 09 | `09.McpAgentsDemo` | MCP servers & custom agents |
| 10 | `10.BlazorChatDemo` | Full-stack Blazor chat app |
| 11 | `11.OpenRouterDemo` | Bring Your Own Key (BYOK) |

## Recording Demos

Each demo (except 10.BlazorChatDemo) includes a `steps/` directory with numbered `.cs` snapshots that build up the final `Program.cs` incrementally. Use the `record-demo.sh` script to automate the recording workflow:

```bash
./record-demo.sh 02.SessionDemo              # uses ~/CopilotDemo by default
./record-demo.sh 03.ToolsDemo ~/Desktop/Demo  # custom output path
```

The script:
1. Creates a fresh `dotnet console` project
2. Copies the `.csproj` with the correct SDK packages
3. Restores NuGet packages
4. Steps through each `.cs` snapshot interactively

**Controls:**
- **Enter** — advance to next step (overwrites `Program.cs`)
- **r** — run the project (`dotnet run`)
- **d** — show diff from previous step
- **s** — show current step info
- **q** — quit

## Prerequisites

- .NET 10 SDK
- GitHub Copilot access (VS Code login or `gh auth login`)