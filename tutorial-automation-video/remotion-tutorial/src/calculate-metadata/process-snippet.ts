import { highlight } from "codehike/code";
import { PublicFolderFile } from "./get-files";
import { Theme } from "./theme";

export const processSnippet = async (step: PublicFolderFile, theme: Theme) => {
  const splitted = step.filename.split(".");
  const extension = splitted[splitted.length - 1];

  // Map .cs to csharp for syntax highlighting
  const lang = extension === "cs" ? "csharp" : extension;

  const highlighted = await highlight(
    {
      lang,
      meta: "",
      value: step.value,
    },
    theme,
  );

  return highlighted;
};
