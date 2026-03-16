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
  /** For title/ending slides: structured data for rendering */
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

const MAX_SLIDE_LINES = 15;
const MAX_LINE_LENGTH = 60; // chars — fits code panel at fontSize 22+

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

  const stepsDir = join(repoRoot, dirName, "steps");
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
 * Parse a step file into its component methods.
 */
interface ParsedMethod {
  name: string;
  comment: string; // e.g. "Paso 3: Ping"
  signature: string;
  body: string[]; // lines of the method body (without { })
  fullLines: string[]; // all lines including signature
}

function parseMethods(content: string): ParsedMethod[] {
  const lines = content.split("\n");
  const methods: ParsedMethod[] = [];

  for (let i = 0; i < lines.length; i++) {
    // Look for method signatures like:
    //   CopilotClient Step1_CreateClient()
    //   async Task Step2_StartClient(CopilotClient client)
    //   public async Task RunAsync()
    const sigMatch = lines[i].match(
      /^\s+(?:public\s+)?(?:async\s+)?(?:Task(?:<\w+>)?\s+|void\s+|CopilotClient\s+)((?:Step\d+_)?\w+)\s*\(/
    );
    if (!sigMatch) continue;

    const methodName = sigMatch[1];

    // Look for preceding comment
    let comment = "";
    for (let j = i - 1; j >= 0 && j >= i - 3; j--) {
      const commentMatch = lines[j].match(
        /\/\/\s*──?\s*(?:Paso\s+\d+:\s*)?(.+?)(?:\s*──|$)/
      );
      if (commentMatch) {
        comment = commentMatch[1].trim();
        break;
      }
    }

    // Find the opening brace
    let braceStart = i;
    while (braceStart < lines.length && !lines[braceStart].includes("{")) {
      braceStart++;
    }

    // Expression-bodied method (=>)
    if (lines[i].includes("=>") && !lines[i].includes("{")) {
      const fullLines = [lines[i]];
      let j = i + 1;
      while (j < lines.length && !lines[j - 1].trimEnd().endsWith(";")) {
        fullLines.push(lines[j]);
        j++;
      }
      methods.push({
        name: methodName,
        comment,
        signature: lines[i].trim(),
        body: fullLines.map((l) => l.trim()),
        fullLines,
      });
      continue;
    }

    if (braceStart >= lines.length) continue;

    // Count braces to find method end
    let depth = 0;
    const fullLines: string[] = [];
    const bodyLines: string[] = [];
    let inBody = false;

    for (let j = i; j < lines.length; j++) {
      fullLines.push(lines[j]);

      for (const ch of lines[j]) {
        if (ch === "{") {
          depth++;
          if (depth === 1) inBody = true;
        }
        if (ch === "}") depth--;
      }

      if (inBody && depth >= 1) {
        // Skip the opening brace line itself
        if (j > braceStart) {
          bodyLines.push(lines[j]);
        }
      }

      if (depth === 0 && inBody) break;
    }

    // Remove the closing brace from body
    if (bodyLines.length > 0 && bodyLines[bodyLines.length - 1].trim() === "}") {
      bodyLines.pop();
    }

    methods.push({
      name: methodName,
      comment,
      signature: lines[i].trim(),
      body: bodyLines,
      fullLines,
    });
  }

  return methods;
}

/**
 * Extract the "interesting" body of a step method.
 * Strips PrintStep calls and Console.WriteLine boilerplate,
 * keeps API calls and logic.
 */
function extractMethodEssence(method: ParsedMethod): string[] {
  return method.body
    .filter((line) => {
      const trimmed = line.trim();
      // Keep empty lines for readability
      if (trimmed === "") return true;
      // Skip PrintStep calls (step announcement)
      if (trimmed.startsWith("PrintStep(")) return false;
      // Skip bare Console.WriteLine() (spacer)
      if (trimmed === "Console.WriteLine();") return false;
      return true;
    })
    .map((line) => {
      // Dedent
      return line;
    });
}

/**
 * Extract slides from step files.
 * Strategy:
 *   - Step 1: show base structure (imports + setup)
 *   - Step 2: show helper methods (if they appear)
 *   - Step 3+: show only the new step method body
 *   - Skip RunInteractiveMode entirely
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

  // Track which methods we've already shown
  const seenMethods = new Set<string>();

  for (let i = 0; i < config.stepFiles.length; i++) {
    const currentPath = join(config.stepsDir, config.stepFiles[i]);
    const currentContent = readFileSync(currentPath, "utf-8");
    const currentMethods = parseMethods(currentContent);

    if (i === 0) {
      // First step: show base structure
      slides.push({
        slideNumber: slideNumber++,
        code: buildBaseStructureSlide(currentContent),
        comment: "Estructura base",
        isSubSlide: false,
        type: "code",
      });
      currentMethods.forEach((m) => seenMethods.add(m.name));
      continue;
    }

    const prevPath = join(config.stepsDir, config.stepFiles[i - 1]);
    const prevContent = readFileSync(prevPath, "utf-8");
    const prevMethods = parseMethods(prevContent);
    const prevMethodNames = new Set(prevMethods.map((m) => m.name));

    // Find new methods in this step
    const newMethods = currentMethods.filter(
      (m) => !prevMethodNames.has(m.name) && !seenMethods.has(m.name)
    );

    // Process new methods
    for (const method of newMethods) {
      // Skip RunAsync (orchestrator) - it just adds calls
      if (method.name === "RunAsync") continue;

      // Skip interactive mode
      if (
        method.name === "RunInteractiveMode" ||
        method.name.includes("Interactive")
      ) {
        slides.push({
          slideNumber: slideNumber++,
          code: `// Modo interactivo\n// Mismo patron que Demo 01\n// (bucle de entrada del usuario)`,
          comment: "Modo interactivo",
          isSubSlide: false,
          type: "code",
        });
        seenMethods.add(method.name);
        continue;
      }

      // Extract the interesting part of the method
      const essence = extractMethodEssence(method);
      const dedented = dedentBlock(essence);
      const comment =
        method.comment ||
        method.name.replace(/^Step\d+_/, "").replace(/([A-Z])/g, " $1").trim();

      const code = `// ${comment}\n${dedented}`;
      const nonEmpty = code
        .split("\n")
        .filter((l) => l.trim() !== "").length;

      if (nonEmpty > MAX_SLIDE_LINES) {
        const subSlides = splitLongCode(code, comment);
        for (const sub of subSlides) {
          slides.push({
            slideNumber: slideNumber++,
            code: sub.code,
            comment: sub.comment,
            isSubSlide: sub.isSubSlide,
            type: "code",
          });
        }
      } else {
        slides.push({
          slideNumber: slideNumber++,
          code,
          comment,
          isSubSlide: false,
          type: "code",
        });
      }

      seenMethods.add(method.name);
    }

    // If no new methods were found, check for significant body changes
    if (newMethods.length === 0) {
      // Check if helper methods were added (not StepN_ methods)
      const newHelpers = currentMethods.filter(
        (m) =>
          !prevMethodNames.has(m.name) &&
          !m.name.startsWith("Step") &&
          m.name !== "RunAsync"
      );

      if (newHelpers.length > 0) {
        // Show helpers
        for (const helper of newHelpers) {
          const code = dedentBlock(helper.fullLines);
          slides.push({
            slideNumber: slideNumber++,
            code: `// ${helper.comment || helper.name}\n${code}`,
            comment: helper.comment || helper.name,
            isSubSlide: false,
            type: "code",
          });
          seenMethods.add(helper.name);
        }
      } else {
        // Show what changed in the diff
        const diffSlide = extractDiffSlide(prevContent, currentContent);
        if (diffSlide) {
          slides.push({
            slideNumber: slideNumber++,
            code: diffSlide.code,
            comment: diffSlide.comment,
            isSubSlide: false,
            type: "code",
          });
        }
      }
    }
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

function buildBaseStructureSlide(content: string): string {
  const parts: string[] = [];

  parts.push(`// Paso 0: Estructura base`);
  parts.push(`#:package GitHub.Copilot.SDK@0.1.23`);
  parts.push(`#:package Microsoft.Extensions.Logging.Console@*`);
  parts.push(``);
  parts.push(`using GitHub.Copilot.SDK;`);
  parts.push(`using Microsoft.Extensions.Logging;`);
  parts.push(``);
  parts.push(`using var loggerFactory = LoggerFactory.Create(b =>`);
  parts.push(`    b.AddConsole().SetMinimumLevel(LogLevel.Warning));`);
  parts.push(``);
  parts.push(`var logger = loggerFactory.CreateLogger<CopilotClient>();`);
  parts.push(``);
  parts.push(`// dotnet run step01.cs`);

  return parts.join("\n");
}

/**
 * When no new methods are found, do a line-by-line diff to find new content.
 */
function extractDiffSlide(
  prevContent: string,
  currentContent: string
): { code: string; comment: string } | null {
  const prevLines = new Set(prevContent.split("\n").map((l) => l.trim()));
  const currentLines = currentContent.split("\n");

  const newLines = currentLines.filter((l) => {
    const trimmed = l.trim();
    if (trimmed === "" || trimmed === "{" || trimmed === "}") return false;
    if (trimmed.startsWith("//")) return false;
    return !prevLines.has(trimmed);
  });

  if (newLines.length === 0) return null;

  // Try to find a meaningful comment from the new lines
  const commentLine = currentContent.split("\n").find((l) => {
    const trimmed = l.trim();
    return (
      trimmed.startsWith("// ── Paso") && !prevContent.includes(trimmed)
    );
  });

  const comment = commentLine
    ? commentLine
        .replace(/\/\/\s*──?\s*(?:Paso\s+\d+:\s*)?/, "")
        .replace(/\s*──.*$/, "")
        .trim()
    : "Cambios";

  const code = dedentBlock(newLines);
  return { code: `// ${comment}\n${code}`, comment };
}

function dedentBlock(lines: string[]): string {
  if (lines.length === 0) return "";

  const nonEmpty = lines.filter((l) => l.trim().length > 0);
  if (nonEmpty.length === 0) return lines.join("\n");

  const minIndent = Math.min(
    ...nonEmpty.map((l) => {
      const match = l.match(/^(\s*)/);
      return match ? match[1].length : 0;
    })
  );

  return lines.map((l) => l.slice(minIndent)).join("\n");
}

interface SubSlide {
  code: string;
  comment: string;
  isSubSlide: boolean;
}

function splitLongCode(code: string, comment: string): SubSlide[] {
  const lines = code.split("\n");
  const subSlides: SubSlide[] = [];
  let current: string[] = [];

  for (const line of lines) {
    current.push(line);

    const isBreakPoint =
      line.trim() === "" || line.trim().startsWith("// ──");
    const nonEmpty = current.filter((l) => l.trim() !== "").length;

    if (isBreakPoint && nonEmpty >= 5 && nonEmpty <= MAX_SLIDE_LINES) {
      subSlides.push({
        code: current.join("\n").trimEnd(),
        comment: subSlides.length === 0 ? comment : `${comment} (cont.)`,
        isSubSlide: subSlides.length > 0,
      });
      current = ["// ..."];
    }
  }

  if (
    current.filter((l) => l.trim() !== "" && l.trim() !== "// ...").length > 0
  ) {
    subSlides.push({
      code: current.join("\n").trimEnd(),
      comment: subSlides.length === 0 ? comment : `${comment} (cont.)`,
      isSubSlide: subSlides.length > 0,
    });
  }

  if (subSlides.length <= 1) {
    // Couldn't split meaningfully - truncate with ellipsis
    const truncated = lines.slice(0, MAX_SLIDE_LINES);
    truncated.push("// ...");
    return [{ code: truncated.join("\n"), comment, isSubSlide: false }];
  }

  return subSlides;
}

/**
 * Wrap lines that exceed maxLen characters.
 * Wraps at the last space/comma/operator before maxLen,
 * continuing with 4-space indent on the next line.
 */
function wrapLongLines(code: string, maxLen: number): string {
  return code
    .split("\n")
    .flatMap((line) => wrapSingleLine(line, maxLen))
    .join("\n");
}

function wrapSingleLine(line: string, maxLen: number): string[] {
  if (line.length <= maxLen) return [line];

  // Measure leading whitespace for continuation indent
  const leadingMatch = line.match(/^(\s*)/);
  const leadingIndent = leadingMatch ? leadingMatch[1] : "";
  const continuationIndent = leadingIndent + "    ";

  const results: string[] = [];
  let remaining = line;

  while (remaining.length > maxLen) {
    // Find a good break point: last space, comma, paren, or operator before maxLen
    let breakAt = -1;
    for (let i = maxLen; i >= maxLen / 2; i--) {
      const ch = remaining[i];
      if (ch === " " || ch === "," || ch === "(" || ch === "+" || ch === "=") {
        breakAt = ch === "," || ch === "(" ? i + 1 : i;
        break;
      }
    }

    if (breakAt === -1) {
      // No good break point — force break at maxLen
      breakAt = maxLen;
    }

    results.push(remaining.slice(0, breakAt).trimEnd());
    remaining = continuationIndent + remaining.slice(breakAt).trimStart();
  }

  if (remaining.trim()) {
    results.push(remaining);
  }

  return results;
}
