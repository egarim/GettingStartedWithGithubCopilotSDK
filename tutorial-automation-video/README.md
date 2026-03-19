# Tutorial Video Generator

Automatically generate code walkthrough videos from any project's step files. Produces syntax-highlighted slides with terminal output, TTS narration, and animated transitions.

**Works with any language** — C#, Python, TypeScript, Rust, Go, Java, etc.

## How It Works

You write code in incremental steps (`step01.cs`, `step02.cs`, ...). Each file is cumulative — it contains all previous code plus new additions. The generator diffs consecutive steps to extract only the new code per slide, adds narration, and produces a video.

```
Your project:                    Generated video:
  step01.py  (setup)       →    Slide 1: Title card
  step02.py  (+ function)  →    Slide 2: Setup code
  step03.py  (+ API call)  →    Slide 3: New function
  step04.py  (+ output)    →    Slide 4: API call
                            →    Slide 5: Output handling
                            →    Slide 6: End card
```

## Quick Start

### 1. Install

```bash
cd tutorial-automation-video/remotion-tutorial
npm install

cd generator
npm install
```

### 2. Generate from any project

```bash
npx tsx cli.ts --source /path/to/my-project \
               --title "My Tutorial" \
               --lang python \
               --pattern "step*.py"
```

### 3. Preview & Record

```bash
# Terminal 1: Remotion Studio
cd remotion-tutorial && npx remotion studio

# Terminal 2: Recorder Studio
cd generator && npm run recorder
```

Open **http://localhost:3456** to preview slides and record narration.

### 4. Render

Trigger the GitHub Actions workflow, or render from Remotion Studio UI.

---

## CLI Reference

```
npx tsx cli.ts [options]
```

### Generic mode (any project)

| Flag | Description | Default |
|------|-------------|---------|
| `--source <dir>` | Directory with step files | (required) |
| `--title <text>` | Video title | directory name |
| `--id <name>` | Composition ID | derived from dir |
| `--lang <lang>` | Syntax highlighting | `csharp` |
| `--pattern <glob>` | Step file pattern | `step*.cs` |
| `--output <dir>` | Output directory | `public/demos/<id>` |
| `--skip-audio` | Skip TTS generation | false |
| `--dry-run` | Preview without writing | false |

### Known demos mode (Copilot SDK)

| Flag | Description |
|------|-------------|
| `--demo <NN>` | Generate one demo (e.g., `--demo 01`) |
| `--all` | Generate all 10 demos |

### Examples

```bash
# C# project
npx tsx cli.ts --source ./my-csharp-tutorial \
               --title "Getting Started with EF Core" \
               --pattern "step*.cs"

# Python project
npx tsx cli.ts --source ./python-ml-tutorial \
               --title "ML with Scikit-learn" \
               --lang python \
               --pattern "step*.py"

# TypeScript project
npx tsx cli.ts --source ./react-tutorial \
               --title "React Hooks" \
               --lang typescript \
               --pattern "lesson*.tsx"

# Rust project
npx tsx cli.ts --source ./rust-basics \
               --title "Rust Ownership" \
               --lang rust \
               --pattern "step*.rs"

# Just preview, no files
npx tsx cli.ts --source ./any-project --dry-run
```

## Supported Languages

Any language supported by [Code Hike](https://codehike.org) / Shiki:

`csharp` `python` `typescript` `javascript` `rust` `go` `java` `cpp` `c`
`ruby` `php` `swift` `kotlin` `scala` `bash` `sql` `json` `yaml` `toml`
`html` `css` `markdown` `dockerfile` and [many more](https://shiki.style/languages).

---

## Step File Format

Each step file must be a **complete, runnable program**. Steps are cumulative — each includes all previous code plus new additions.

```
my-tutorial/
  step01.py    ← base setup (imports, config)
  step02.py    ← step01 + first feature
  step03.py    ← step02 + second feature
  step04.py    ← step03 + third feature
```

The generator diffs `step02` vs `step01` to extract the new code for slide 2, and so on.

### Tips for good slides

- **Add a comment** at the top of each new section: `// Paso 3: Ping` or `# Step 3: API call`
- **Keep diffs small** — 5-15 new lines per step is ideal
- **Boilerplate is auto-stripped** — imports, package directives, and common setup lines are filtered out

---

## Recorder Studio

Browser-based tool for recording your own narration per slide.

```bash
cd generator
npm run recorder
# Open http://localhost:3456
```

### Features

- Per-slide narration script, code preview, and console output
- Record / Play / Stop per slide
- Play All for sequential playback
- Auto duration sync after recording
- Keyboard shortcuts: `R` record, `Space` play, `P` play all, `↑↓` navigate, `Esc` stop

### Configuration

```bash
# Point at any demos directory
npx tsx recorder-server.ts /path/to/demos

# Environment variables
DEMOS_DIR=/path/to/demos
REMOTION_PORT=3000
RECORDER_PORT=3456
```

---

## Rendering

### GitHub Actions (recommended)

Push your code and trigger the **Render Tutorial Videos** workflow from the Actions tab. Videos are uploaded as artifacts.

### Remotion Studio UI

Open `http://localhost:3000`, select a composition, and use the render button.

### CLI (x64 only)

```bash
npx remotion render Demo01 --output=out/Demo01.mp4
```

> Note: CLI rendering requires x64 architecture. Windows ARM64 users should use GitHub Actions or Remotion Studio UI.

---

## Architecture

```
generator/
  cli.ts                  ← Main CLI (generic + known demos)
  recorder-server.ts      ← Recording studio server
  recorder.html           ← Recording studio UI
  src/
    extract-slides.ts     ← Diffs step files → slides
    generate-narration.ts ← TTS via Edge TTS
    build-manifest.ts     ← Builds manifest.json
    console-outputs.ts    ← Terminal output per slide
    demo-descriptions.ts  ← Titles/narration for known demos

remotion-tutorial/
  src/
    Root.tsx              ← Auto-discovers compositions from public/demos/
    DemoVideo.tsx         ← Video component (loads data via delayRender)
    CodeTransition.tsx    ← Syntax-highlighted code panel
    Console.tsx           ← Terminal output panel
    TitleSlide.tsx        ← Title/ending cards
    ProgressBar.tsx       ← Step progress indicator
  public/
    demos/                ← Generated content (one folder per video)
      demo01/
        manifest.json
        slides/
        audio/
```
