import { mkdirSync, readdirSync, existsSync } from "fs";
import { join, resolve, basename } from "path";
import {
  extractSlides,
  type DemoConfig,
} from "./src/extract-slides.js";
import {
  generateAudio,
  generateNarrationScript,
} from "./src/generate-narration.js";
import {
  buildManifest,
  writeManifestAndSlides,
} from "./src/build-manifest.js";
import { getConsoleOutputs } from "./src/console-outputs.js";
import { DESCRIPTIONS } from "./src/demo-descriptions.js";

// Default paths (for the Copilot SDK project)
const DEFAULT_REPO_ROOT = resolve(import.meta.dirname, "..", "..", "..");
const PUBLIC_DIR = resolve(import.meta.dirname, "..", "public", "demos");

// Known demo directories (for --all and --demo NN shortcuts)
const KNOWN_DEMOS: Record<string, string> = {
  "01": "01.ClientDemo",
  "02": "02.SessionDemo",
  "03": "03.ToolsDemo",
  "04": "04.HooksDemo",
  "05": "05.PermissionsDemo",
  "06": "06.AskUserDemo",
  "07": "07.CompactionDemo",
  "08": "08.SkillsDemo",
  "09": "09.McpAgentsDemo",
  "11": "11.OpenRouterDemo",
};

interface CliOptions {
  // Legacy mode (known demos)
  demo?: string;
  all: boolean;
  // Generic mode
  source?: string;
  title?: string;
  id?: string;
  lang?: string;
  pattern?: string;
  // Shared
  output?: string;
  skipAudio: boolean;
  dryRun: boolean;
}

function parseArgs(): CliOptions {
  const args = process.argv.slice(2);
  const options: CliOptions = {
    all: false,
    skipAudio: false,
    dryRun: false,
    lang: "csharp",
    pattern: "step*.cs",
  };

  for (let i = 0; i < args.length; i++) {
    switch (args[i]) {
      case "--demo":     options.demo = args[++i]; break;
      case "--all":      options.all = true; break;
      case "--source":   options.source = args[++i]; break;
      case "--title":    options.title = args[++i]; break;
      case "--id":       options.id = args[++i]; break;
      case "--lang":     options.lang = args[++i]; break;
      case "--pattern":  options.pattern = args[++i]; break;
      case "--output":   options.output = args[++i]; break;
      case "--skip-audio": options.skipAudio = true; break;
      case "--dry-run":  options.dryRun = true; break;
      case "--help":
        console.log(`Tutorial Video Generator

Usage:
  npx tsx cli.ts --source <dir> --title "My Tutorial" [options]
  npx tsx cli.ts --demo <NN>                          (known demos)
  npx tsx cli.ts --all                                (all known demos)

Generic mode (any project):
  --source <dir>    Directory containing step files
  --title <text>    Video title (e.g., "Python Basics")
  --id <name>       Composition ID (default: derived from dir name)
  --lang <lang>     Syntax highlighting language (default: csharp)
                    Options: csharp, python, typescript, javascript,
                    go, rust, java, cpp, bash, sql, json, yaml, etc.
  --pattern <glob>  Step file pattern (default: "step*.cs")
                    Examples: "step*.py", "*.ts", "lesson*.rs"
  --output <dir>    Output directory (default: public/demos/<id>)

Known demos mode (Copilot SDK project):
  --demo <NN>       Generate for a known demo (e.g., --demo 01)
  --all             Generate for all known demos

Shared options:
  --skip-audio      Skip TTS audio generation
  --dry-run         Preview slides without writing files
  --help            Show this help

Examples:
  # Any project
  npx tsx cli.ts --source ./my-python-tutorial --title "Learn Python" --lang python --pattern "step*.py"
  npx tsx cli.ts --source ./rust-demos/basics --title "Rust Basics" --lang rust --pattern "lesson*.rs"

  # Known Copilot SDK demos
  npx tsx cli.ts --demo 01
  npx tsx cli.ts --all --skip-audio`);
        process.exit(0);
    }
  }

  if (!options.demo && !options.all && !options.source) {
    console.error("Error: specify --source <dir>, --demo <NN>, or --all");
    console.error("Run with --help for usage.");
    process.exit(1);
  }

  return options;
}

/**
 * Build a DemoConfig from a generic source directory.
 */
function buildGenericConfig(options: CliOptions): DemoConfig {
  const sourceDir = resolve(options.source!);
  if (!existsSync(sourceDir)) {
    throw new Error(`Source directory not found: ${sourceDir}`);
  }

  // Determine file pattern
  const patternRegex = new RegExp(
    "^" + options.pattern!.replace("*", ".*") + "$"
  );
  const stepFiles = readdirSync(sourceDir)
    .filter((f) => patternRegex.test(f))
    .sort();

  if (stepFiles.length === 0) {
    throw new Error(
      `No files matching "${options.pattern}" in ${sourceDir}`
    );
  }

  const dirName = basename(sourceDir);
  const id = options.id ?? dirName.toLowerCase().replace(/[^a-z0-9]/g, "-");

  return {
    demoId: id,
    demoNumber: "",
    demoName: dirName,
    stepsDir: sourceDir,
    stepFiles,
  };
}

/**
 * Build a DemoConfig for a known Copilot SDK demo.
 */
function buildKnownConfig(demoNumber: string): DemoConfig {
  const dirName = KNOWN_DEMOS[demoNumber];
  if (!dirName) {
    throw new Error(
      `Unknown demo: ${demoNumber}. Known: ${Object.keys(KNOWN_DEMOS).join(", ")}`
    );
  }

  const stepsDir = join(DEFAULT_REPO_ROOT, dirName);
  const stepFiles = readdirSync(stepsDir)
    .filter((f) => /^step\d+\.cs$/.test(f))
    .sort();

  return {
    demoId: `demo${demoNumber}`,
    demoNumber,
    demoName: dirName.split(".")[1],
    stepsDir,
    stepFiles,
  };
}

async function processConfig(
  config: DemoConfig,
  title: string,
  options: CliOptions
): Promise<void> {
  console.log(`\n${"=".repeat(60)}`);
  console.log(`Processing: ${title}`);
  console.log(`${"=".repeat(60)}`);
  console.log(`  Source: ${config.stepsDir}`);
  console.log(`  Files:  ${config.stepFiles.length} (${config.stepFiles[0]} ... ${config.stepFiles[config.stepFiles.length - 1]})`);
  console.log(`  ID:     ${config.demoId}`);

  // Extract slides
  console.log("\n  Extracting slides...");
  const slides = extractSlides(config, title);
  console.log(`  Extracted ${slides.length} slides`);

  if (options.dryRun) {
    console.log("\n  [DRY RUN] Slides:");
    for (const slide of slides) {
      const tag = slide.type !== "code" ? ` [${slide.type}]` : "";
      console.log(`    Slide ${slide.slideNumber}: ${slide.comment}${tag}`);
      console.log(`      ${slide.code.split("\n").length} lines`);
    }
    return;
  }

  // Output directory
  const outputDir = options.output
    ? resolve(options.output)
    : join(PUBLIC_DIR, config.demoId);
  mkdirSync(outputDir, { recursive: true });

  // Narration
  const desc = config.demoNumber ? DESCRIPTIONS[config.demoNumber] : undefined;
  const narrationTexts = slides.map((s) => {
    if (s.type === "title" && desc) return desc.narration;
    if (s.type === "ending" && desc) return desc.endNarration;
    return generateNarrationScript(s, title);
  });

  const narrationOverrides: Record<number, string> = {};
  slides.forEach((s, i) => {
    narrationOverrides[s.slideNumber] = narrationTexts[i];
  });

  // Audio
  let audioResults;
  if (options.skipAudio) {
    console.log("\n  Skipping audio generation");
    audioResults = slides.map((s) => ({
      slideNumber: s.slideNumber,
      audioFile: `slide${String(s.slideNumber).padStart(2, "0")}.mp3`,
      durationMs: 5000,
      durationFrames: 210,
    }));
  } else {
    console.log("\n  Generating audio...");
    audioResults = await generateAudio(slides, title, outputDir, narrationOverrides);
  }

  // Console outputs
  const consoleOutputs = config.demoNumber
    ? getConsoleOutputs(config.demoNumber, slides.length, slides)
    : slides.map(() => "");

  // Build manifest
  console.log("\n  Building manifest...");
  const manifest = buildManifest(config.demoId, title, slides, audioResults, consoleOutputs, narrationTexts);
  writeManifestAndSlides(manifest, slides, outputDir);

  console.log(`\n  Done! ${manifest.totalFrames} frames (~${Math.round(manifest.totalFrames / 30)}s)`);
  console.log(`  Output: ${outputDir}`);
}

async function main() {
  const options = parseArgs();

  if (options.source) {
    // Generic mode
    const config = buildGenericConfig(options);
    const title = options.title ?? config.demoName;
    await processConfig(config, title, options);
  } else {
    // Known demos mode
    const demoNumbers = options.all
      ? Object.keys(KNOWN_DEMOS).sort()
      : [options.demo!.padStart(2, "0")];

    for (const num of demoNumbers) {
      const config = buildKnownConfig(num);
      const title = DESCRIPTIONS[num]?.title
        ? `${num} - ${DESCRIPTIONS[num].title}`
        : `Demo ${num}`;
      await processConfig(config, title, options);
    }
  }

  console.log("\n\nAll done!");
}

main().catch((err) => {
  console.error("Fatal error:", err);
  process.exit(1);
});
