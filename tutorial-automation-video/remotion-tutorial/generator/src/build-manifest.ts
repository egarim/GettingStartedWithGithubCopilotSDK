import { writeFileSync, mkdirSync } from "fs";
import { join } from "path";
import type { SlideContent, SlideType } from "./extract-slides.js";
import type { AudioResult } from "./generate-narration.js";

export interface SlideManifest {
  file: string;
  audioFile: string;
  durationFrames: number;
  narrationText: string;
  consoleOutput: string;
  type: SlideType;
  cardData?: {
    title: string;
    subtitle?: string;
    bullets?: string[];
    nextTitle?: string;
  };
}

export interface DemoManifest {
  demoId: string;
  title: string;
  fps: number;
  totalFrames: number;
  slides: SlideManifest[];
}

/**
 * Build a manifest.json for a demo, combining slides, audio results,
 * and console outputs.
 */
export function buildManifest(
  demoId: string,
  title: string,
  slides: SlideContent[],
  audioResults: AudioResult[],
  consoleOutputs: string[],
  narrationTexts: string[]
): DemoManifest {
  const fps = 30;

  const slideManifests: SlideManifest[] = slides.map((slide, index) => {
    const audio = audioResults.find((a) => a.slideNumber === slide.slideNumber);
    const slideFile = `slide${String(slide.slideNumber).padStart(2, "0")}.cs`;

    return {
      file: slideFile,
      audioFile:
        audio?.audioFile ??
        `slide${String(slide.slideNumber).padStart(2, "0")}.mp3`,
      durationFrames: audio?.durationFrames ?? 180,
      narrationText: narrationTexts[index] ?? "",
      consoleOutput: consoleOutputs[index] ?? "",
      type: slide.type,
      ...(slide.cardData ? { cardData: slide.cardData } : {}),
    };
  });

  const totalFrames = slideManifests.reduce(
    (sum, s) => sum + s.durationFrames,
    0
  );

  return {
    demoId,
    title,
    fps,
    totalFrames,
    slides: slideManifests,
  };
}

/**
 * Write manifest and slide files to the output directory.
 */
export function writeManifestAndSlides(
  manifest: DemoManifest,
  slides: SlideContent[],
  outputDir: string
): void {
  const slidesDir = join(outputDir, "slides");
  mkdirSync(slidesDir, { recursive: true });
  mkdirSync(join(outputDir, "audio"), { recursive: true });

  const manifestPath = join(outputDir, "manifest.json");
  writeFileSync(manifestPath, JSON.stringify(manifest, null, 2), "utf-8");
  console.log(`  Written: ${manifestPath}`);

  for (const slide of slides) {
    const fileName = `slide${String(slide.slideNumber).padStart(2, "0")}.cs`;
    const filePath = join(slidesDir, fileName);
    writeFileSync(filePath, slide.code, "utf-8");
  }
  console.log(`  Written: ${slides.length} slide files to ${slidesDir}`);
}
