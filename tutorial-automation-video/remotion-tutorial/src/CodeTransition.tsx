import { HighlightedCode, Pre } from "codehike/code";
import React, { useMemo } from "react";
import {
  fontFamily,
  fontSize as defaultFontSize,
  tabSize,
} from "./font";

export function CodeTransition({
  oldCode,
  newCode,
  fontSize: fontSizeOverride,
}: {
  readonly oldCode: HighlightedCode | null;
  readonly newCode: HighlightedCode;
  readonly durationInFrames?: number;
  readonly fontSize?: number;
}) {
  const size = fontSizeOverride ?? defaultFontSize;

  const style: React.CSSProperties = useMemo(() => {
    return {
      position: "relative",
      fontSize: size,
      lineHeight: 1.5,
      fontFamily,
      tabSize,
    };
  }, [size]);

  return <Pre code={newCode} handlers={[]} style={style} />;
}
