import { getThemeColors } from "@code-hike/lighter";
import { measureText } from "@remotion/layout-utils";
import { HighlightedCode } from "codehike/code";
import { CalculateMetadataFunction } from "remotion";
import { z } from "zod";
import {
  fontFamily,
  fontSize,
  tabSize,
  waitUntilDone,
} from "../font";
import { Props } from "../Main";
import { getFiles } from "./get-files";
import { processSnippet } from "./process-snippet";
import { schema } from "./schema";

export const calculateMetadata: CalculateMetadataFunction<
  Props & z.infer<typeof schema>
> = async ({ props }) => {
  const contents = await getFiles();

  await waitUntilDone();
  const widthPerCharacter = measureText({
    text: "A",
    fontFamily,
    fontSize,
    validateFontIsLoaded: true,
  }).width;

  const maxCharacters = Math.max(
    ...contents
      .map(({ value }) => value.split("\n"))
      .flat()
      .map((value) => value.replaceAll("\t", " ".repeat(tabSize)).length)
      .flat(),
  );
  const codeWidth = widthPerCharacter * maxCharacters;

  // Step durations matched to audio lengths (frames @ 30fps)
  const stepDurations = [528, 431, 404, 372, 357, 313, 310, 415, 428];
  const totalDuration = stepDurations.reduce((a, b) => a + b, 0);

  const themeColors = await getThemeColors(props.theme);

  const twoSlashedCode: HighlightedCode[] = [];
  for (const snippet of contents) {
    twoSlashedCode.push(await processSnippet(snippet, props.theme));
  }

  return {
    durationInFrames: totalDuration,
    width: 1920,
    props: {
      theme: props.theme,
      width: props.width,
      steps: twoSlashedCode,
      themeColors,
      codeWidth: Math.min(codeWidth, 1840),
    },
  };
};

/**
 * Calculate metadata for a manifest-driven demo composition.
 * Reads slides and durations from the demo's manifest.json.
 */
export function createDemoCalculateMetadata(demoId: string) {
  const calc: CalculateMetadataFunction<any> = async ({ props }) => {
    const contents = await getFiles(demoId);

    // If no slides found for this demo, fall back to empty
    if (contents.length === 0) {
      return {
        durationInFrames: 300,
        width: 1920,
        props: {
          ...props,
          demoId,
          steps: null,
          themeColors: null,
          codeWidth: 1080,
          stepDurations: [300],
          audioFiles: [],
          consoleOutputs: [],
        },
      };
    }

    await waitUntilDone();
    const widthPerCharacter = measureText({
      text: "A",
      fontFamily,
      fontSize,
      validateFontIsLoaded: true,
    }).width;

    const maxCharacters = Math.max(
      ...contents
        .map(({ value }) => value.split("\n"))
        .flat()
        .map((value) => value.replaceAll("\t", " ".repeat(tabSize)).length)
        .flat(),
    );
    const codeWidth = widthPerCharacter * maxCharacters;

    const themeColors = await getThemeColors(props.theme ?? "github-dark");

    const twoSlashedCode: HighlightedCode[] = [];
    for (const snippet of contents) {
      twoSlashedCode.push(
        await processSnippet(snippet, props.theme ?? "github-dark")
      );
    }

    // Load manifest for durations, audio, console outputs
    const { getManifest } = await import("./get-files");
    const manifest = await getManifest(demoId);

    let stepDurations: number[];
    let audioFiles: string[];
    let consoleOutputs: string[];
    let slideMetadata: { type: string; cardData?: unknown }[];

    if (manifest) {
      stepDurations = manifest.slides.map(
        (s: { durationFrames: number }) => s.durationFrames
      );
      audioFiles = manifest.slides.map(
        (s: { audioFile: string }) => `demos/${demoId}/audio/${s.audioFile}`
      );
      consoleOutputs = manifest.slides.map(
        (s: { consoleOutput: string }) => s.consoleOutput
      );
      slideMetadata = manifest.slides.map(
        (s: { type?: string; cardData?: unknown }) => ({
          type: s.type ?? "code",
          cardData: s.cardData,
        })
      );
    } else {
      // Default: uniform durations, no audio
      const defaultDuration = 210; // 7 seconds
      stepDurations = twoSlashedCode.map(() => defaultDuration);
      audioFiles = [];
      consoleOutputs = twoSlashedCode.map(() => "");
      slideMetadata = twoSlashedCode.map(() => ({ type: "code" }));
    }

    const totalDuration = stepDurations.reduce((a, b) => a + b, 0);

    return {
      durationInFrames: totalDuration,
      width: 1920,
      props: {
        ...props,
        demoId,
        steps: twoSlashedCode,
        themeColors,
        codeWidth: Math.min(codeWidth, 1840),
        stepDurations,
        audioFiles,
        consoleOutputs,
        slideMetadata,
      },
    };
  };

  return calc;
}
