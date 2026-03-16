# Tutorial Video Automation Pipeline

Automated video generation for the GitHub Copilot SDK tutorial series. Produces code walkthrough videos with syntax-highlighted slides, terminal output, narration, and animated transitions.

## Architecture

```
tutorial-automation-video/
  remotion-tutorial/          # Remotion video project
    src/                      # React components (video rendering)
    public/demos/             # Generated content per demo
      demo01/
        manifest.json         # Slide metadata, durations, console outputs
        slides/               # Code snippets (.cs files)
        audio/                # Narration audio (.mp3/.webm)
      demo02/ ... demo11/
    generator/                # CLI tools
      cli.ts                  # Main generator (slides + TTS + manifest)
      recorder-server.ts      # Browser-based audio recorder
      recorder.html           # Recorder UI
      src/                    # Generator modules
```

## Quick Start

### 1. Install dependencies

```bash
cd remotion-tutorial
npm install

cd generator
npm install
```

### 2. Generate all demos (with TTS narration)

```bash
cd generator
npx tsx cli.ts --all
```

This extracts code slides from the step files, generates Spanish TTS audio, and builds manifests.

### 3. Preview in browser

```bash
# Terminal 1: Start Remotion Studio
cd remotion-tutorial
npx remotion studio

# Terminal 2: Start Recorder Studio
cd remotion-tutorial/generator
npm run recorder
```

Open **http://localhost:3456** — integrated preview + audio recorder.

### 4. Record your own narration

In the Recorder Studio (localhost:3456):
- Select a demo from the dropdown
- Click a slide to expand it and see the narration script
- Click **Record** (or press `R`) to record your voice
- Click **Stop** to save — duration auto-syncs
- Press **Space** to play back, `P` to play all slides

### 5. Render final videos

```bash
cd remotion-tutorial
npx remotion render Demo01    # Render one demo
npx remotion render Demo07    # Render another
```

Output: `out/Demo01.mp4` (1920x1080, 30fps)

---

## Generator CLI

```bash
cd generator
npx tsx cli.ts [options]
```

| Option | Description |
|--------|-------------|
| `--demo NN` | Generate for a single demo (e.g., `--demo 01`) |
| `--all` | Generate for all 10 demos |
| `--skip-audio` | Skip TTS generation (slides + manifest only) |
| `--dry-run` | Preview what would be generated |

### What it does

1. **Extracts slides** from `XX.DemoName/steps/stepNN.cs` files by diffing consecutive steps
2. **Adds title/ending slides** with demo name, description, and "next video" pointer
3. **Generates TTS audio** using Edge TTS (`es-MX-DaliaNeural` voice)
4. **Measures audio duration** and calculates frame counts (30fps + 2s padding)
5. **Builds manifest.json** with slides, durations, console outputs, and narration text

### Slide extraction strategy

- Each step file is compared to the previous one
- New methods are extracted, boilerplate is stripped
- Methods > 15 lines are split into sub-slides with `// ...` continuation
- `RunInteractiveMode` methods are skipped (shown as summary slide)
- Title slide added at start, ending slide at end

---

## Recorder Studio

Browser-based tool for recording narration per slide with live Remotion preview.

```bash
cd generator
npm run recorder
# or: npx tsx recorder-server.ts [demos-dir]
```

### Features

- **Split view**: Remotion preview (left) + slide list (right)
- **Per-slide recording**: Record, play, stop with one click
- **Auto duration sync**: After recording, manifest updates automatically
- **Remotion auto-reload**: Preview refreshes to match new durations
- **Keyboard shortcuts**:
  - `R` — Record / Stop recording
  - `Space` — Play / Stop current slide
  - `P` — Play all slides sequentially
  - `↑↓` — Navigate slides
  - `Esc` — Stop everything

### Configuration

| Env Variable | Default | Description |
|-------------|---------|-------------|
| `DEMOS_DIR` | `../public/demos` | Path to demos directory |
| `REMOTION_PORT` | `3000` | Remotion Studio port |
| `RECORDER_PORT` | `3456` | Recorder server port |

Or pass the demos directory as a CLI argument:

```bash
npx tsx recorder-server.ts /path/to/any/demos/directory
```

---

## Remotion Components

### `DemoVideo.tsx`
Main video component. Reads manifest data (slides, durations, console outputs, audio paths) and renders:
- **Title/ending slides** — full-screen animated cards (`TitleSlide.tsx`)
- **Code slides** — syntax-highlighted C# with `CodeTransition.tsx` (left 65%) + terminal output with `Console.tsx` (right 35%)
- **Progress bar** — visual step indicator with variable durations
- **Audio** — synced narration per slide via Remotion `<Sequence>`

### `TitleSlide.tsx`
Animated title card with:
- Demo name and subtitle
- Bullet points (what you'll learn)
- Fade-in animations with accent bar

### Font scaling
Code panel automatically scales font size (30px default, minimum 16px) based on the longest line to prevent horizontal cutoff.

---

## Manifest Format

Each demo has a `manifest.json`:

```json
{
  "demoId": "demo01",
  "title": "01 - Ciclo de vida y conexion del cliente",
  "fps": 30,
  "totalFrames": 3654,
  "slides": [
    {
      "file": "slide01.cs",
      "audioFile": "slide01.mp3",
      "durationFrames": 420,
      "narrationText": "Demo uno. Ciclo de vida...",
      "consoleOutput": "=== 1. Creando CopilotClient ===\n  Estado: Disconnected",
      "type": "title",
      "cardData": {
        "title": "Demo 01 — Ciclo de vida del cliente",
        "subtitle": "Conectar, consultar y desconectar",
        "bullets": ["Crear CopilotClient", "StartAsync", "Ping", "StopAsync"]
      }
    }
  ]
}
```

### Slide types

| Type | Rendering |
|------|-----------|
| `title` | Full-screen card with title, subtitle, bullets |
| `code` | Split view: syntax-highlighted code + terminal |
| `ending` | Full-screen card pointing to next demo |

---

## Adding a New Demo

1. Create step files in `XX.NewDemo/steps/step01.cs` through `stepNN.cs`
2. Add the demo number to `generator/src/extract-slides.ts` (`DEMO_DIRS` and `DEMO_TITLES`)
3. Add console outputs to `generator/src/console-outputs.ts`
4. Add title/narration to `generator/src/demo-descriptions.ts`
5. Add narration map entries to `generator/src/generate-narration.ts`
6. Add the demo ID to `src/Root.tsx` (`DEMO_IDS` array)
7. Run `npx tsx cli.ts --demo XX`

---

## Demo Summary

| Demo | Topic | Steps | Slides | Duration |
|------|-------|-------|--------|----------|
| 01 | Client lifecycle | 9 | 11 | ~2 min |
| 02 | Sessions & multi-turn | 12 | 17 | ~3 min |
| 03 | Custom tools (AIFunction) | 7 | 14 | ~2 min |
| 04 | Pre/Post tool hooks | 6 | 8 | ~1.5 min |
| 05 | Permission handling | 9 | 20 | ~3 min |
| 06 | AskUser interaction | 5 | 13 | ~2 min |
| 07 | Context compaction | 4 | 16 | ~2 min |
| 08 | Skills (SKILL.md) | 5 | 13 | ~2 min |
| 09 | MCP servers & agents | 10 | 23 | ~3 min |
| 11 | BYOK / OpenRouter | 8 | 13 | ~2 min |
