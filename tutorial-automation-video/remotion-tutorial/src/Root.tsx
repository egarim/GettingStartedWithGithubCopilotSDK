import { Composition } from "remotion";
import { DemoVideo } from "./DemoVideo";

// Demo configs: id + estimated duration (from manifest totalFrames)
// DemoVideo loads its own data at render time via delayRender,
// so Remotion Studio doesn't choke evaluating all compositions on startup.
const DEMOS = [
  { id: "demo01", frames: 3654 },
  { id: "demo02", frames: 4959 },
  { id: "demo03", frames: 3841 },
  { id: "demo04", frames: 2449 },
  { id: "demo05", frames: 4700 },
  { id: "demo06", frames: 3135 },
  { id: "demo07", frames: 3424 },
  { id: "demo08", frames: 3137 },
  { id: "demo09", frames: 5373 },
  { id: "demo11", frames: 3506 },
];

export const RemotionRoot = () => {
  return (
    <>
      {DEMOS.map(({ id, frames }) => (
        <Composition
          key={id}
          id={id.charAt(0).toUpperCase() + id.slice(1)}
          component={DemoVideo}
          defaultProps={{ demoId: id }}
          fps={30}
          width={1920}
          height={1080}
          durationInFrames={frames}
        />
      ))}
    </>
  );
};
