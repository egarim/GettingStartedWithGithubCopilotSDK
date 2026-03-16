import { mkdirSync } from "fs";
import { join, resolve } from "path";
import {
  extractSlides,
  getAllDemoNumbers,
  getDemoConfig,
  getDemoTitle,
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

// Paths
const REPO_ROOT = resolve(import.meta.dirname, "..", "..", "..");
const PUBLIC_DIR = resolve(import.meta.dirname, "..", "public", "demos");

interface CliOptions {
  demo?: string;
  all: boolean;
  skipAudio: boolean;
  dryRun: boolean;
}

function parseArgs(): CliOptions {
  const args = process.argv.slice(2);
  const options: CliOptions = {
    all: false,
    skipAudio: false,
    dryRun: false,
  };

  for (let i = 0; i < args.length; i++) {
    switch (args[i]) {
      case "--demo":
        options.demo = args[++i];
        break;
      case "--all":
        options.all = true;
        break;
      case "--skip-audio":
        options.skipAudio = true;
        break;
      case "--dry-run":
        options.dryRun = true;
        break;
      case "--help":
        console.log(`Usage: npx tsx cli.ts [options]

Options:
  --demo <NN>     Generate for a single demo (e.g., --demo 01)
  --all           Generate for all demos
  --skip-audio    Skip audio generation (slides + manifest only)
  --dry-run       Show what would be generated without writing files
  --help          Show this help message

Examples:
  npx tsx cli.ts --demo 01
  npx tsx cli.ts --all --skip-audio
  npx tsx cli.ts --demo 03 --dry-run`);
        process.exit(0);
    }
  }

  if (!options.demo && !options.all) {
    console.error("Error: specify --demo <NN> or --all");
    process.exit(1);
  }

  return options;
}

async function processDemo(
  demoNumber: string,
  options: CliOptions
): Promise<void> {
  console.log(`\n${"=".repeat(60)}`);
  console.log(`Processing Demo ${demoNumber}`);
  console.log(`${"=".repeat(60)}`);

  // 1. Get demo config
  const config = getDemoConfig(demoNumber, REPO_ROOT);
  const title = getDemoTitle(demoNumber);
  const desc = DESCRIPTIONS[demoNumber];
  console.log(
    `  Demo: ${config.demoName} (${config.stepFiles.length} steps)`
  );

  // 2. Extract slides (now includes title + ending)
  console.log("\n  Extracting slides...");
  const slides = extractSlides(config);
  console.log(`  Extracted ${slides.length} slides`);

  if (options.dryRun) {
    console.log("\n  [DRY RUN] Slides:");
    for (const slide of slides) {
      const tag = slide.type !== "code" ? ` [${slide.type}]` : "";
      console.log(`    Slide ${slide.slideNumber}: ${slide.comment}${tag}`);
      const lines = slide.code.split("\n").length;
      console.log(
        `      ${lines} lines${slide.isSubSlide ? " (sub-slide)" : ""}`
      );
    }
    return;
  }

  // 3. Output directory
  const outputDir = join(PUBLIC_DIR, config.demoId);
  mkdirSync(outputDir, { recursive: true });

  // 4. Generate narration texts — use custom narration for title/ending
  const narrationTexts = slides.map((s) => {
    if (s.type === "title" && desc) return desc.narration;
    if (s.type === "ending" && desc) return desc.endNarration;
    return generateNarrationScript(s, title);
  });

  // 5. Build narration overrides map for audio generation
  const narrationOverrides: Record<number, string> = {};
  slides.forEach((s, i) => {
    narrationOverrides[s.slideNumber] = narrationTexts[i];
  });

  // 6. Generate audio (or skip)
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
    audioResults = await generateAudio(
      slides,
      title,
      outputDir,
      narrationOverrides
    );
  }

  // 7. Get console outputs (pad for title/ending slides)
  const consoleOutputs = getConsoleOutputs(demoNumber, slides.length);

  // 8. Build and write manifest
  console.log("\n  Building manifest...");
  const manifest = buildManifest(
    config.demoId,
    title,
    slides,
    audioResults,
    consoleOutputs,
    narrationTexts
  );

  writeManifestAndSlides(manifest, slides, outputDir);

  console.log(
    `\n  Done! Total frames: ${manifest.totalFrames} (~${Math.round(manifest.totalFrames / 30)}s)`
  );
}

async function main() {
  const options = parseArgs();

  const demos = options.all
    ? getAllDemoNumbers()
    : [options.demo!.padStart(2, "0")];

  for (const demo of demos) {
    await processDemo(demo, options);
  }

  console.log("\n\nAll done!");
}

main().catch((err) => {
  console.error("Fatal error:", err);
  process.exit(1);
});
