import { getThemeColors } from "@code-hike/lighter";
import { HighlightedCode } from "codehike/code";
import { useCallback, useEffect, useState } from "react";
import {
  AbsoluteFill,
  Audio,
  continueRender,
  delayRender,
  Sequence,
  Series,
  staticFile,
} from "remotion";
import { CodeTransition } from "./CodeTransition";
import { Console } from "./Console";
import { ProgressBar } from "./ProgressBar";
import { CardData, TitleSlide } from "./TitleSlide";
import { ThemeColors, ThemeProvider } from "./calculate-metadata/theme";
import { getFiles, getManifest } from "./calculate-metadata/get-files";
import { processSnippet } from "./calculate-metadata/process-snippet";
import {
  fontFamily,
  fontSize as defaultFontSize,
  horizontalPadding,
  tabSize,
  verticalPadding,
  waitUntilDone,
} from "./font";
import { measureText } from "@remotion/layout-utils";

export type SlideType = "code" | "title" | "ending";

export interface SlideMetadata {
  type: SlideType;
  cardData?: CardData;
}

export type DemoVideoProps = {
  demoId: string;
};

const VIDEO_WIDTH = 1920;
const CODE_PANEL_RATIO = 0.65;
const CODE_PANEL_WIDTH = VIDEO_WIDTH * CODE_PANEL_RATIO;
const AVAILABLE_CODE_WIDTH = CODE_PANEL_WIDTH - horizontalPadding * 2;
const MIN_FONT_SIZE = 16;

interface LoadedData {
  steps: HighlightedCode[];
  themeColors: ThemeColors;
  codeWidth: number;
  stepDurations: number[];
  audioFiles: string[];
  consoleOutputs: string[];
  slideMetadata: SlideMetadata[];
}

export const DemoVideo: React.FC<DemoVideoProps> = ({ demoId }) => {
  const [data, setData] = useState<LoadedData | null>(null);
  const [handle] = useState(() => delayRender("Loading demo data..."));

  const loadData = useCallback(async () => {
    try {
      const manifest = await getManifest(demoId);
      const contents = await getFiles(demoId);

      if (!contents.length || !manifest) {
        continueRender(handle);
        return;
      }

      await waitUntilDone();

      const widthPerChar = measureText({
        text: "A", fontFamily, fontSize: defaultFontSize, validateFontIsLoaded: true,
      }).width;

      const maxChars = Math.max(
        ...contents.flatMap(({ value }) =>
          value.split("\n").map((l) => l.replaceAll("\t", " ".repeat(tabSize)).length)
        )
      );

      const themeColors = await getThemeColors("github-dark");

      const steps: HighlightedCode[] = [];
      for (const snippet of contents) {
        steps.push(await processSnippet(snippet, "github-dark"));
      }

      setData({
        steps,
        themeColors,
        codeWidth: Math.min(widthPerChar * maxChars, 1840),
        stepDurations: manifest.slides.map((s: any) => s.durationFrames),
        audioFiles: manifest.slides.map((s: any) => `demos/${demoId}/audio/${s.audioFile}`),
        consoleOutputs: manifest.slides.map((s: any) => s.consoleOutput ?? ""),
        slideMetadata: manifest.slides.map((s: any) => ({
          type: s.type ?? "code",
          cardData: s.cardData,
        })),
      });

      continueRender(handle);
    } catch (err) {
      console.error("DemoVideo load error:", err);
      continueRender(handle);
    }
  }, [demoId, handle]);

  useEffect(() => { loadData(); }, [loadData]);

  if (!data) {
    return (
      <AbsoluteFill style={{ backgroundColor: "#1e1e2e", display: "flex", alignItems: "center", justifyContent: "center" }}>
        <div style={{ color: "#58a6ff", fontSize: 32, fontFamily: "monospace" }}>
          Loading {demoId}...
        </div>
      </AbsoluteFill>
    );
  }

  const { steps, themeColors, codeWidth, stepDurations, audioFiles, consoleOutputs, slideMetadata } = data;
  const transitionDuration = 30;

  const fontSize = codeWidth > 0
    ? Math.max(MIN_FONT_SIZE, Math.min(Math.floor(defaultFontSize * AVAILABLE_CODE_WIDTH / codeWidth), defaultFontSize))
    : defaultFontSize;

  const stepStartFrames = stepDurations.reduce<number[]>((acc, dur, i) => {
    acc.push(i === 0 ? 0 : acc[i - 1] + stepDurations[i - 1]);
    return acc;
  }, []);

  const consolePanelWidth = `${(1 - CODE_PANEL_RATIO) * 100}%`;
  const codePanelWidth = `${CODE_PANEL_RATIO * 100}%`;

  return (
    <ThemeProvider themeColors={themeColors}>
      <AbsoluteFill style={{ backgroundColor: themeColors.background }}>
        <Series>
          {steps.map((step, index) => {
            const meta = slideMetadata[index];
            const isCard = meta?.type === "title" || meta?.type === "ending";

            return (
              <Series.Sequence
                key={index}
                layout="none"
                durationInFrames={stepDurations[index] || 180}
                name={step.meta}
              >
                {isCard && meta?.cardData ? (
                  <AbsoluteFill>
                    <TitleSlide cardData={meta.cardData} variant={meta.type as "title" | "ending"} />
                  </AbsoluteFill>
                ) : (
                  <AbsoluteFill>
                    <div style={{ display: "flex", width: "100%", height: "100%" }}>
                      <div style={{ width: codePanelWidth, height: "100%", position: "relative", overflow: "hidden" }}>
                        <ProgressBar steps={steps} stepDurations={stepDurations} />
                        <div style={{ padding: `${verticalPadding}px 0px`, position: "absolute", top: 0, left: 0, right: 0, bottom: 0 }}>
                          <CodeTransition oldCode={steps[index - 1] ?? null} newCode={step} durationInFrames={transitionDuration} fontSize={fontSize} />
                        </div>
                      </div>
                      <div style={{ width: consolePanelWidth, height: "100%" }}>
                        <Console output={consoleOutputs[index] ?? ""} stepStartFrame={0} />
                      </div>
                    </div>
                  </AbsoluteFill>
                )}
              </Series.Sequence>
            );
          })}
        </Series>
      </AbsoluteFill>

      {audioFiles.map((file, index) => (
        <Sequence key={file} from={stepStartFrames[index]} durationInFrames={stepDurations[index] || 180}>
          <Audio src={staticFile(file)} volume={1} />
        </Sequence>
      ))}
    </ThemeProvider>
  );
};
