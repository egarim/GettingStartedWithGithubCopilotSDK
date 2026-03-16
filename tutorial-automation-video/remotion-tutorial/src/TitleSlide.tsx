import React, { useMemo } from "react";
import { interpolate, useCurrentFrame } from "remotion";
import { useThemeColors } from "./calculate-metadata/theme";

export interface CardData {
  title: string;
  subtitle?: string;
  bullets?: string[];
  nextTitle?: string;
}

export const TitleSlide: React.FC<{
  cardData: CardData;
  variant: "title" | "ending";
}> = ({ cardData, variant }) => {
  const frame = useCurrentFrame();
  const themeColors = useThemeColors();

  const titleOpacity = interpolate(frame, [0, 20], [0, 1], {
    extrapolateRight: "clamp",
  });

  const subtitleOpacity = interpolate(frame, [15, 35], [0, 1], {
    extrapolateRight: "clamp",
  });

  const bulletsOpacity = interpolate(frame, [30, 50], [0, 1], {
    extrapolateRight: "clamp",
  });

  const titleSlide = interpolate(frame, [0, 25], [30, 0], {
    extrapolateRight: "clamp",
  });

  const accentColor = themeColors?.icon?.foreground ?? "#58a6ff";
  const textColor = themeColors?.foreground ?? "#e0e0e0";
  const bgColor = themeColors?.background ?? "#1e1e2e";
  const subtitleColor = themeColors?.editor?.lineHighlightBackground ?? "#888";

  const containerStyle: React.CSSProperties = useMemo(
    () => ({
      display: "flex",
      flexDirection: "column",
      justifyContent: "center",
      alignItems: variant === "ending" ? "center" : "flex-start",
      height: "100%",
      width: "100%",
      padding: "80px 100px",
      backgroundColor: bgColor,
      boxSizing: "border-box",
    }),
    [bgColor, variant]
  );

  if (variant === "ending") {
    return (
      <div style={containerStyle}>
        <div
          style={{
            opacity: titleOpacity,
            transform: `translateY(${titleSlide}px)`,
            textAlign: "center",
          }}
        >
          <div
            style={{
              fontSize: 56,
              fontWeight: 700,
              color: accentColor,
              fontFamily: "'Roboto Mono', monospace",
              marginBottom: 30,
            }}
          >
            {cardData.title}
          </div>
        </div>
        {cardData.subtitle && (
          <div
            style={{
              opacity: subtitleOpacity,
              fontSize: 38,
              color: textColor,
              fontFamily: "'Roboto Mono', monospace",
              textAlign: "center",
            }}
          >
            {cardData.subtitle}
          </div>
        )}
      </div>
    );
  }

  // Title variant
  return (
    <div style={containerStyle}>
      {/* Accent bar */}
      <div
        style={{
          width: interpolate(frame, [0, 30], [0, 120], {
            extrapolateRight: "clamp",
          }),
          height: 4,
          backgroundColor: accentColor,
          marginBottom: 30,
          borderRadius: 2,
        }}
      />

      {/* Demo title */}
      <div
        style={{
          opacity: titleOpacity,
          transform: `translateY(${titleSlide}px)`,
        }}
      >
        <div
          style={{
            fontSize: 54,
            fontWeight: 700,
            color: textColor,
            fontFamily: "'Roboto Mono', monospace",
            marginBottom: 16,
            lineHeight: 1.2,
          }}
        >
          {cardData.title}
        </div>
      </div>

      {/* Subtitle */}
      {cardData.subtitle && (
        <div
          style={{
            opacity: subtitleOpacity,
            fontSize: 32,
            color: typeof subtitleColor === "string" ? subtitleColor : "#888",
            fontFamily: "'Roboto Mono', monospace",
            marginBottom: 50,
          }}
        >
          {cardData.subtitle}
        </div>
      )}

      {/* Bullet points */}
      {cardData.bullets && cardData.bullets.length > 0 && (
        <div style={{ opacity: bulletsOpacity }}>
          {cardData.bullets.map((bullet, i) => (
            <div
              key={i}
              style={{
                fontSize: 28,
                color: textColor,
                fontFamily: "'Roboto Mono', monospace",
                marginBottom: 14,
                paddingLeft: 20,
                opacity: interpolate(
                  frame,
                  [35 + i * 8, 50 + i * 8],
                  [0, 1],
                  { extrapolateRight: "clamp" }
                ),
                transform: `translateX(${interpolate(
                  frame,
                  [35 + i * 8, 50 + i * 8],
                  [20, 0],
                  { extrapolateRight: "clamp" }
                )}px)`,
              }}
            >
              <span style={{ color: accentColor, marginRight: 12 }}>
                {"->"}
              </span>
              {bullet}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
