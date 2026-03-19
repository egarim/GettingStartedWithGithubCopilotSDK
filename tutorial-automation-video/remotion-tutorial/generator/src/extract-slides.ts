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
  demoNumber: string;
  demoName: string;
  stepsDir: string;
  stepFiles: string[];
}

// Demo directory name mapping
const DEMO_DIRS: Record<string, string> = {
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

const DEMO_TITLES: Record<string, string> = {
  "01": "01 - Ciclo de vida y conexion del cliente",
  "02": "02 - Sesiones y conversaciones multi-turno",
  "03": "03 - Tools: funciones personalizadas",
  "04": "04 - Hooks: pre y post ejecucion",
  "05": "05 - Permisos y autorizacion",
  "06": "06 - AskUser: interaccion con el usuario",
  "07": "07 - Compactacion de contexto",
  "08": "08 - Skills: habilidades del SDK",
  "09": "09 - MCP Agents",
  "11": "11 - OpenRouter: modelos BYOK",
};

export function getDemoTitle(demoNumber: string): string {
  return DEMO_TITLES[demoNumber] ?? `Demo ${demoNumber}`;
}

export function getDemoConfig(
  demoNumber: string,
  repoRoot: string
): DemoConfig {
  const dirName = DEMO_DIRS[demoNumber];
  if (!dirName) {
    throw new Error(
      `Unknown demo number: ${demoNumber}. Valid: ${Object.keys(DEMO_DIRS).join(", ")}`
    );
  }

  // Step files are at the demo root (single-file apps)
  const stepsDir = join(repoRoot, dirName);
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

export function getAllDemoNumbers(): string[] {
  return Object.keys(DEMO_DIRS).sort();
}

/**
 * Lines to strip from diffs (boilerplate that repeats in every step)
 */
const BOILERPLATE_LINES = new Set([
  "#:package GitHub.Copilot.SDK@0.1.23",
  "#:package Microsoft.Extensions.Logging.Console@*",
  "#:package Microsoft.Extensions.AI@*",
  "using GitHub.Copilot.SDK;",
  "using Microsoft.Extensions.Logging;",
  "using Microsoft.Extensions.AI;",
  "using System.Text;",
  "using System.ComponentModel;",
  "using System.Text.Json;",
  "using System.Text.Json.Serialization;",
  "",
  "using var loggerFactory = LoggerFactory.Create(b =>",
  "b.AddConsole().SetMinimumLevel(LogLevel.Warning));",
  "var logger = loggerFactory.CreateLogger<CopilotClient>();",
  "await client.DisposeAsync();",
]);

function isBoilerplate(line: string): boolean {
  const trimmed = line.trim();
  return BOILERPLATE_LINES.has(trimmed);
}

/**
 * Extract the comment from a code block (first // comment line)
 */
function extractComment(lines: string[]): string {
  for (const line of lines) {
    const match = line.trim().match(/^\/\/\s*(?:Paso\s+\d+:\s*)?(.+)/);
    if (match) return match[1].trim();
  }
  return "";
}

/**
 * Extract slides from single-file app step files.
 * Each step is a cumulative file — we diff consecutive steps
 * to find the new lines added.
 */
export function extractSlides(config: DemoConfig): SlideContent[] {
  const slides: SlideContent[] = [];
  let slideNumber = 1;

  // === Title slide ===
  const desc = DESCRIPTIONS[config.demoNumber];
  if (desc) {
    const bulletText = desc.bullets.map((b) => `// - ${b}`).join("\n");
    slides.push({
      slideNumber: slideNumber++,
      code: `// ${desc.title}\n// ${desc.subtitle}\n//\n${bulletText}`,
      comment: desc.title,
      isSubSlide: false,
      type: "title",
      cardData: {
        title: `Demo ${config.demoNumber} — ${desc.title}`,
        subtitle: desc.subtitle,
        bullets: desc.bullets,
      },
    });
  }

  for (let i = 0; i < config.stepFiles.length; i++) {
    const currentPath = join(config.stepsDir, config.stepFiles[i]);
    const currentContent = readFileSync(currentPath, "utf-8");

    if (i === 0) {
      // First step: use the file content directly (it's the base structure)
      slides.push({
        slideNumber: slideNumber++,
        code: currentContent.trim(),
        comment: "Estructura base",
        isSubSlide: false,
        type: "code",
      });
      continue;
    }

    // Diff against previous step to find new lines
    const prevPath = join(config.stepsDir, config.stepFiles[i - 1]);
    const prevContent = readFileSync(prevPath, "utf-8");
    const prevLines = new Set(prevContent.split("\n").map((l) => l.trimEnd()));

    const currentLines = currentContent.split("\n");
    const newLines: string[] = [];

    for (const line of currentLines) {
      if (!prevLines.has(line.trimEnd()) && !isBoilerplate(line)) {
        newLines.push(line);
      }
    }

    // Trim leading/trailing empty lines
    while (newLines.length > 0 && newLines[0].trim() === "") newLines.shift();
    while (newLines.length > 0 && newLines[newLines.length - 1].trim() === "") newLines.pop();

    if (newLines.length === 0) continue;

    const code = newLines.join("\n");
    const comment = extractComment(newLines) || `Paso ${i}`;

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
    const endingTitle = next
      ? `Siguiente: Demo ${next.number}`
      : "Serie completada!";
    const endingSubtitle = next ? next.title : "GitHub Copilot SDK";

    slides.push({
      slideNumber: slideNumber++,
      code: `// ${endingTitle}\n// ${endingSubtitle}`,
      comment: endingTitle,
      isSubSlide: false,
      type: "ending",
      cardData: {
        title: endingTitle,
        subtitle: endingSubtitle,
        nextTitle: next?.title,
      },
    });
  }

  return slides;
}
