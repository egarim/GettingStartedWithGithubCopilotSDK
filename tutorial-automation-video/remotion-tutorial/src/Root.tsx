import { Composition } from "remotion";
import { getStaticFiles } from "@remotion/studio";
import { DemoVideo } from "./DemoVideo";

/**
 * Auto-discover demos from public/demos/ directory.
 * Any folder with a manifest.json becomes a composition.
 */
function discoverDemos(): { id: string; frames: number }[] {
  try {
    const files = getStaticFiles();
    const manifests = files.filter(
      (f) => f.name.match(/^demos\/[^/]+\/manifest\.json$/)
    );

    return manifests.map((f) => {
      const demoId = f.name.split("/")[1]; // e.g. "demo01"
      // Default duration — actual duration comes from delayRender in DemoVideo
      return { id: demoId, frames: 3000 };
    }).sort((a, b) => a.id.localeCompare(b.id));
  } catch {
    // Fallback if getStaticFiles not available (e.g. during render)
    return [];
  }
}

// Capitalize first letter: "demo01" -> "Demo01"
function toCompId(id: string): string {
  return id.charAt(0).toUpperCase() + id.slice(1);
}

export const RemotionRoot = () => {
  const demos = discoverDemos();

  // If no demos discovered, register a placeholder
  if (demos.length === 0) {
    return (
      <Composition
        id="NoDemo"
        component={DemoVideo}
        defaultProps={{ demoId: "demo01" }}
        fps={30}
        width={1920}
        height={1080}
        durationInFrames={300}
      />
    );
  }

  return (
    <>
      {demos.map(({ id, frames }) => (
        <Composition
          key={id}
          id={toCompId(id)}
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
