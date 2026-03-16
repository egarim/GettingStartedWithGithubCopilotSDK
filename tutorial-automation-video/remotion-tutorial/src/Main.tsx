import { HighlightedCode } from "codehike/code";
import { useMemo } from "react";
import {
  AbsoluteFill,
  Audio,
  Sequence,
  Series,
  staticFile,
  useCurrentFrame,
} from "remotion";
import { CodeTransition } from "./CodeTransition";
import { Console } from "./Console";
import { consoleOutputs } from "./consoleOutput";
import { ProgressBar } from "./ProgressBar";
import { RefreshOnCodeChange } from "./ReloadOnCodeChange";
import { ThemeColors, ThemeProvider } from "./calculate-metadata/theme";
import { verticalPadding } from "./font";

export type Props = {
  steps: HighlightedCode[] | null;
  themeColors: ThemeColors | null;
  codeWidth: number | null;
};

// Duration per step in frames (audio length + 2s padding @ 30fps)
const stepDurations = [528, 431, 404, 372, 357, 313, 310, 415, 428];

const audioFiles = [
  "audio/step1.mp3",
  "audio/step2.mp3",
  "audio/step3.mp3",
  "audio/step4.mp3",
  "audio/step5.mp3",
  "audio/step6.mp3",
  "audio/step7.mp3",
  "audio/step8.mp3",
  "audio/step9.mp3",
];

// Precompute cumulative start frames
const stepStartFrames = stepDurations.reduce<number[]>((acc, dur, i) => {
  acc.push(i === 0 ? 0 : acc[i - 1] + stepDurations[i - 1]);
  return acc;
}, []);

export const Main: React.FC<Props> = ({ steps, themeColors, codeWidth }) => {
  if (!steps) {
    throw new Error("Steps are not defined");
  }

  const frame = useCurrentFrame();
  const transitionDuration = 30;

  // Find current step based on variable durations
  let currentStepIndex = 0;
  for (let i = 0; i < stepStartFrames.length; i++) {
    if (frame >= stepStartFrames[i]) {
      currentStepIndex = i;
    }
  }

  if (!themeColors) {
    throw new Error("Theme colors are not defined");
  }

  const outerStyle: React.CSSProperties = useMemo(() => {
    return {
      backgroundColor: themeColors.background,
    };
  }, [themeColors]);

  const consoleOutput = consoleOutputs[currentStepIndex] ?? "";

  return (
    <ThemeProvider themeColors={themeColors}>
      <AbsoluteFill style={outerStyle}>
        {/* Split layout: code left 60%, console right 40% */}
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
              width: "60%",
              height: "100%",
              position: "relative",
              overflow: "hidden",
            }}
          >
            <ProgressBar steps={steps} />
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
              <Series>
                {steps.map((step, index) => (
                  <Series.Sequence
                    key={index}
                    layout="none"
                    durationInFrames={stepDurations[index] || 180}
                    name={step.meta}
                  >
                    <CodeTransition
                      oldCode={steps[index - 1]}
                      newCode={step}
                      durationInFrames={transitionDuration}
                    />
                  </Series.Sequence>
                ))}
              </Series>
            </div>
          </div>

          {/* Console panel */}
          <div style={{ width: "40%", height: "100%" }}>
            <Console
              output={consoleOutput}
              stepStartFrame={stepStartFrames[currentStepIndex]}
            />
          </div>
        </div>
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
