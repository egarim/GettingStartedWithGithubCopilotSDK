import { HighlightedCode } from "codehike/code";
import { useMemo } from "react";
import {
  AbsoluteFill,
  Audio,
  Sequence,
  Series,
  staticFile,
} from "remotion";
import { CodeTransition } from "./CodeTransition";
import { Console } from "./Console";
import { ProgressBar } from "./ProgressBar";
import { RefreshOnCodeChange } from "./ReloadOnCodeChange";
import { CardData, TitleSlide } from "./TitleSlide";
import { ThemeColors, ThemeProvider } from "./calculate-metadata/theme";
import {
  fontSize as defaultFontSize,
  horizontalPadding,
  verticalPadding,
} from "./font";

export type SlideType = "code" | "title" | "ending";

export interface SlideMetadata {
  type: SlideType;
  cardData?: CardData;
}

export type DemoVideoProps = {
  demoId: string;
  steps: HighlightedCode[] | null;
  themeColors: ThemeColors | null;
  codeWidth: number | null;
  stepDurations: number[];
  audioFiles: string[];
  consoleOutputs: string[];
  slideMetadata?: SlideMetadata[];
};

// Video dimensions
const VIDEO_WIDTH = 1920;
const CODE_PANEL_RATIO = 0.65;
const CODE_PANEL_WIDTH = VIDEO_WIDTH * CODE_PANEL_RATIO;
const AVAILABLE_CODE_WIDTH = CODE_PANEL_WIDTH - horizontalPadding * 2;
const MIN_FONT_SIZE = 16;

export const DemoVideo: React.FC<DemoVideoProps> = ({
  demoId,
  steps,
  themeColors,
  codeWidth,
  stepDurations,
  audioFiles,
  consoleOutputs,
  slideMetadata,
}) => {
  if (!steps) {
    throw new Error("Steps are not defined");
  }

  if (!themeColors) {
    throw new Error("Theme colors are not defined");
  }

  const transitionDuration = 30;

  // Calculate font size to fit the widest line in the code panel
  const fontSize = useMemo(() => {
    if (!codeWidth || codeWidth <= 0) return defaultFontSize;
    const scaleFactor = AVAILABLE_CODE_WIDTH / codeWidth;
    const scaled = Math.floor(defaultFontSize * scaleFactor);
    return Math.max(MIN_FONT_SIZE, Math.min(scaled, defaultFontSize));
  }, [codeWidth]);

  // Precompute cumulative start frames
  const stepStartFrames = useMemo(
    () =>
      stepDurations.reduce<number[]>((acc, dur, i) => {
        acc.push(i === 0 ? 0 : acc[i - 1] + stepDurations[i - 1]);
        return acc;
      }, []),
    [stepDurations]
  );

  const outerStyle: React.CSSProperties = useMemo(() => {
    return {
      backgroundColor: themeColors.background,
    };
  }, [themeColors]);

  const consolePanelWidth = `${(1 - CODE_PANEL_RATIO) * 100}%`;
  const codePanelWidth = `${CODE_PANEL_RATIO * 100}%`;

  return (
    <ThemeProvider themeColors={themeColors}>
      <AbsoluteFill style={outerStyle}>
        <Series>
          {steps.map((step, index) => {
            const meta = slideMetadata?.[index];
            const isCard =
              meta?.type === "title" || meta?.type === "ending";

            return (
              <Series.Sequence
                key={index}
                layout="none"
                durationInFrames={stepDurations[index] || 180}
                name={step.meta}
              >
                {isCard && meta?.cardData ? (
                  /* Full-screen title/ending card */
                  <AbsoluteFill>
                    <TitleSlide
                      cardData={meta.cardData}
                      variant={meta.type as "title" | "ending"}
                    />
                  </AbsoluteFill>
                ) : (
                  /* Split layout: code + console */
                  <AbsoluteFill>
                    <div
                      style={{
                        display: "flex",
                        width: "100%",
                        height: "100%",
                      }}
                    >
                      {/* Code panel */}
                      <div
                        style={{
                          width: codePanelWidth,
                          height: "100%",
                          position: "relative",
                          overflow: "hidden",
                        }}
                      >
                        <ProgressBar
                          steps={steps}
                          stepDurations={stepDurations}
                        />
                        <div
                          style={{
                            padding: `${verticalPadding}px 0px`,
                            position: "absolute",
                            top: 0,
                            left: 0,
                            right: 0,
                            bottom: 0,
                          }}
                        >
                          <CodeTransition
                            oldCode={steps[index - 1] ?? null}
                            newCode={step}
                            durationInFrames={transitionDuration}
                            fontSize={fontSize}
                          />
                        </div>
                      </div>

                      {/* Console panel */}
                      <div
                        style={{
                          width: consolePanelWidth,
                          height: "100%",
                        }}
                      >
                        <Console
                          output={consoleOutputs[index] ?? ""}
                          stepStartFrame={0}
                        />
                      </div>
                    </div>
                  </AbsoluteFill>
                )}
              </Series.Sequence>
            );
          })}
        </Series>
      </AbsoluteFill>

      {/* Narration audio — synced to variable step durations */}
      {audioFiles.map((file, index) => (
        <Sequence
          key={file}
          from={stepStartFrames[index]}
          durationInFrames={stepDurations[index] || 180}
        >
          <Audio src={staticFile(file)} volume={1} />
        </Sequence>
      ))}

      <RefreshOnCodeChange />
    </ThemeProvider>
  );
};
