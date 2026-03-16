import { loadFont } from "@remotion/google-fonts/RobotoMono";

export const { fontFamily, waitUntilDone } = loadFont("normal", {
  subsets: ["latin"],
  weights: ["400", "700"],
});
export const fontSize = 30;
export const tabSize = 4;
export const horizontalPadding = 40;
export const verticalPadding = 60;
