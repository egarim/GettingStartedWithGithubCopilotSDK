import { readFileSync, readdirSync } from "fs";
import { join } from "path";
import { DESCRIPTIONS, getNextDemo } from "./demo-descriptions.js";

export type SlideType = "code" | "title" | "ending";

export interface SlideContent {
  slideNumber: number;
  code: string;
  comment: string;
  isSubSlide: boolean;
  type: SlideType;
  cardData?: {
    title: string;
    subtitle?: string;
    bullets?: string[];
    nextTitle?: string;
  };
}

export interface DemoConfig {
  demoId: string;
  demoNumber: string; // empty for generic projects
  demoName: string;
  stepsDir: string;
  stepFiles: string[];
}

/**
 * Common boilerplate lines to ignore when diffing steps.
 * These appear in every cumulative step file and aren't interesting.
 */
const BOILERPLATE_PATTERNS = [
  /^#:package\s/,
  /^using\s+\w/,
  /^using\s+var\s+loggerFactory/,
  /^\s*b\.AddConsole\(\)/,
  /^var\s+logger\s*=/,
  /^await\s+client\.DisposeAsync/,
  /^await\s+client\.StopAsync/,
  /^\s*$/,
];

function isBoilerplate(line: string): boolean {
  const trimmed = line.trim();
  if (trimmed === "") return true;
  return BOILERPLATE_PATTERNS.some((p) => p.test(trimmed));
}

/**
 * Extract the first comment from a code block.
 * Supports // comments and # comments (Python, bash).
 */
function extractComment(lines: string[]): string {
  for (const line of lines) {
    // C#/JS/Java/Go/Rust style
    const slashMatch = line.trim().match(/^\/\/\s*(?:Paso\s+\d+:\s*|Step\s+\d+:\s*)?(.+)/);
    if (slashMatch) return slashMatch[1].trim();
    // Python/bash style
    const hashMatch = line.trim().match(/^#\s*(?:Step\s+\d+:\s*)?(.+)/);
    if (hashMatch && !line.trim().startsWith("#:")) return hashMatch[1].trim();
  }
  return "";
}

/**
 * Extract slides from step files (any language).
 * Each step is cumulative — we diff consecutive files to find new code.
 */
export function extractSlides(config: DemoConfig, title?: string): SlideContent[] {
  const slides: SlideContent[] = [];
  let slideNumber = 1;

  // === Title slide ===
  const desc = config.demoNumber ? DESCRIPTIONS[config.demoNumber] : undefined;
  const videoTitle = title ?? desc?.title ?? config.demoName;

  if (desc) {
    slides.push({
      slideNumber: slideNumber++,
      code: `// ${desc.title}\n// ${desc.subtitle}\n//\n${desc.bullets.map((b) => `// - ${b}`).join("\n")}`,
      comment: desc.title,
      isSubSlide: false,
      type: "title",
      cardData: {
        title: `Demo ${config.demoNumber} — ${desc.title}`,
        subtitle: desc.subtitle,
        bullets: desc.bullets,
      },
    });
  } else {
    // Generic title slide
    slides.push({
      slideNumber: slideNumber++,
      code: `// ${videoTitle}`,
      comment: videoTitle,
      isSubSlide: false,
      type: "title",
      cardData: {
        title: videoTitle,
        subtitle: `${config.stepFiles.length} steps`,
      },
    });
  }

  // === Code slides ===
  for (let i = 0; i < config.stepFiles.length; i++) {
    const currentPath = join(config.stepsDir, config.stepFiles[i]);
    const currentContent = readFileSync(currentPath, "utf-8");

    if (i === 0) {
      slides.push({
        slideNumber: slideNumber++,
        code: currentContent.trim(),
        comment: "Estructura base",
        isSubSlide: false,
        type: "code",
      });
      continue;
    }

    // Diff against previous step
    const prevPath = join(config.stepsDir, config.stepFiles[i - 1]);
    const prevContent = readFileSync(prevPath, "utf-8");
    const prevLines = new Set(prevContent.split("\n").map((l) => l.trimEnd()));

    const newLines: string[] = [];
    for (const line of currentContent.split("\n")) {
      if (!prevLines.has(line.trimEnd()) && !isBoilerplate(line)) {
        newLines.push(line);
      }
    }

    // Trim empty edges
    while (newLines.length > 0 && newLines[0].trim() === "") newLines.shift();
    while (newLines.length > 0 && newLines[newLines.length - 1].trim() === "") newLines.pop();

    if (newLines.length === 0) continue;

    const code = newLines.join("\n");
    const comment = extractComment(newLines) || `Step ${i}`;

    slides.push({
      slideNumber: slideNumber++,
      code,
      comment,
      isSubSlide: false,
      type: "code",
    });
  }

  // === Ending slide ===
  if (desc) {
    const next = getNextDemo(config.demoNumber);
    const endingTitle = next ? `Siguiente: Demo ${next.number}` : "Serie completada!";
    const endingSubtitle = next ? next.title : "GitHub Copilot SDK";
    slides.push({
      slideNumber: slideNumber++,
      code: `// ${endingTitle}\n// ${endingSubtitle}`,
      comment: endingTitle,
      isSubSlide: false,
      type: "ending",
      cardData: { title: endingTitle, subtitle: endingSubtitle, nextTitle: next?.title },
    });
  } else {
    slides.push({
      slideNumber: slideNumber++,
      code: `// End`,
      comment: "End",
      isSubSlide: false,
      type: "ending",
      cardData: { title: "Done!", subtitle: videoTitle },
    });
  }

  return slides;
}
