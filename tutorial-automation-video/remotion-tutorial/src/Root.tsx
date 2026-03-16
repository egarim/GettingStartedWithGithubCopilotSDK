import { Composition } from "remotion";
import { DemoVideo } from "./DemoVideo";
import { createDemoCalculateMetadata } from "./calculate-metadata/calculate-metadata";

// All demo IDs (matching folder names in public/demos/)
const DEMO_IDS = [
  "howto",
  "demo01",
  "demo02",
  "demo03",
  "demo04",
  "demo05",
  "demo06",
  "demo07",
  "demo08",
  "demo09",
  "demo11",
];

const defaultDemoProps = {
  demoId: "",
  steps: null,
  themeColors: null,
  theme: "github-dark" as const,
  codeWidth: null,
  width: { type: "auto" as const },
  stepDurations: [],
  audioFiles: [],
  consoleOutputs: [],
  slideMetadata: [],
};

export const RemotionRoot = () => {
  return (
    <>
      {DEMO_IDS.map((demoId) => (
        <Composition
          key={demoId}
          id={demoId.charAt(0).toUpperCase() + demoId.slice(1)}
          component={DemoVideo}
          defaultProps={{
            ...defaultDemoProps,
            demoId,
          }}
          fps={30}
          width={1920}
          height={1080}
          calculateMetadata={createDemoCalculateMetadata(demoId)}
        />
      ))}
    </>
  );
};
