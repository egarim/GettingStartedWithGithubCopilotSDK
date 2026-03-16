import React, { useMemo } from "react";
import { useCurrentFrame, useVideoConfig } from "remotion";
import { useThemeColors } from "./calculate-metadata/theme";

const Step: React.FC<{
  readonly index: number;
  readonly currentStep: number;
  readonly currentStepProgress: number;
}> = ({ index, currentStep, currentStepProgress }) => {
  const themeColors = useThemeColors();

  const outer: React.CSSProperties = useMemo(() => {
    return {
      backgroundColor:
        themeColors.editor.lineHighlightBackground ??
        themeColors.editor.rangeHighlightBackground,
      borderRadius: 6,
      overflow: "hidden",
      height: "100%",
      flex: 1,
    };
  }, [themeColors]);

  const inner: React.CSSProperties = useMemo(() => {
    return {
      height: "100%",
      backgroundColor: themeColors.icon.foreground,
      width:
        index > currentStep
          ? 0
          : index === currentStep
            ? currentStepProgress * 100 + "%"
            : "100%",
    };
  }, [themeColors.icon.foreground, index, currentStep, currentStepProgress]);

  return (
    <div style={outer}>
      <div style={inner} />
    </div>
  );
};

export function ProgressBar({
  steps,
  stepDurations,
}: {
  readonly steps: unknown[];
  readonly stepDurations?: number[];
}) {
  const frame = useCurrentFrame();
  const { durationInFrames } = useVideoConfig();

  let currentStep: number;
  let currentStepProgress: number;

  if (stepDurations && stepDurations.length === steps.length) {
    // Variable durations mode
    let accumulated = 0;
    currentStep = 0;
    for (let i = 0; i < stepDurations.length; i++) {
      if (frame >= accumulated) {
        currentStep = i;
      }
      accumulated += stepDurations[i];
    }
    const stepStart = stepDurations
      .slice(0, currentStep)
      .reduce((a, b) => a + b, 0);
    currentStepProgress =
      (frame - stepStart) / stepDurations[currentStep];
  } else {
    // Uniform durations fallback
    const stepDuration = durationInFrames / steps.length;
    currentStep = Math.floor(frame / stepDuration);
    currentStepProgress = (frame % stepDuration) / stepDuration;
  }

  const container: React.CSSProperties = useMemo(() => {
    return {
      position: "absolute",
      top: 48,
      height: 6,
      left: 0,
      right: 0,
      display: "flex",
      gap: 12,
    };
  }, []);

  return (
    <div style={container}>
      {steps.map((_, index) => (
        <Step
          key={index}
          currentStep={currentStep}
          currentStepProgress={currentStepProgress}
          index={index}
        />
      ))}
    </div>
  );
}
