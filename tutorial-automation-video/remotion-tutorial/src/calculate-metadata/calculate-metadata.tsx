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
import { getFiles, getManifest } from "./get-files";
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
 * Lightweight calculateMetadata for manifest-driven demos.
 * Only reads the manifest for duration — defers heavy code processing
 * to render time so Remotion Studio doesn't choke loading 10 demos at once.
 */
export function createDemoCalculateMetadata(demoId: string) {
  const calc: CalculateMetadataFunction<any> = async ({ props }) => {
    // Step 1: Read manifest FIRST (cheap — just a JSON fetch)
    const manifest = await getManifest(demoId);

    let totalDuration = 300;
    let stepDurations: number[] = [300];
    let audioFiles: string[] = [];
    let consoleOutputs: string[] = [];
    let slideMetadata: { type: string; cardData?: unknown }[] = [{ type: "code" }];

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
      totalDuration = stepDurations.reduce((a, b) => a + b, 0);
    }

    // Step 2: Load and process code files (expensive but needed for rendering)
    const contents = await getFiles(demoId);

    if (contents.length === 0) {
      return {
        durationInFrames: totalDuration,
        width: 1920,
        props: {
          ...props,
          demoId,
          steps: null,
          themeColors: null,
          codeWidth: 1080,
          stepDurations,
          audioFiles,
          consoleOutputs,
          slideMetadata,
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
