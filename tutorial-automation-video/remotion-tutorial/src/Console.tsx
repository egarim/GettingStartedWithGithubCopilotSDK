import React, { useMemo } from "react";
import { interpolate, useCurrentFrame } from "remotion";
import { useThemeColors } from "./calculate-metadata/theme";

export const Console: React.FC<{
  output: string;
  stepStartFrame: number;
}> = ({ output, stepStartFrame }) => {
  const frame = useCurrentFrame();
  const themeColors = useThemeColors();

  // Fade in the console output after the code transition
  const opacity = interpolate(
    frame - stepStartFrame,
    [40, 55],
    [0, 1],
    { extrapolateLeft: "clamp", extrapolateRight: "clamp" },
  );

  const containerStyle: React.CSSProperties = useMemo(() => ({
    display: "flex",
    flexDirection: "column",
    height: "100%",
    borderLeft: `2px solid ${themeColors?.foreground ?? "#444"}`,
    backgroundColor: "#1a1a2e",
  }), [themeColors]);

  const headerStyle: React.CSSProperties = useMemo(() => ({
    padding: "12px 20px",
    fontSize: 18,
    fontFamily: "monospace",
    color: "#888",
    borderBottom: "1px solid #333",
    display: "flex",
    alignItems: "center",
    gap: 8,
  }), []);

  const outputStyle: React.CSSProperties = useMemo(() => ({
    padding: "20px",
    fontSize: 20,
    lineHeight: 1.6,
    fontFamily: "'Roboto Mono', monospace",
    color: "#e0e0e0",
    whiteSpace: "pre-wrap",
    opacity,
    flex: 1,
  }), [opacity]);

  return (
    <div style={containerStyle}>
      <div style={headerStyle}>
        <span style={{ color: "#50fa7b" }}>●</span> Terminal
      </div>
      <div style={outputStyle}>{output}</div>
    </div>
  );
};
