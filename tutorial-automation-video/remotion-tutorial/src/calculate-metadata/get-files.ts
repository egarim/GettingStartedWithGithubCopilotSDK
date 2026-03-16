import { getStaticFiles } from "@remotion/studio";

export type PublicFolderFile = {
  filename: string;
  value: string;
};

/**
 * Load code files from a demo-specific directory (public/demos/{demoId}/slides/)
 * or fall back to the legacy flat layout (public/code*.cs).
 */
export const getFiles = async (demoId?: string) => {
  const files = getStaticFiles();

  let codeFiles;
  if (demoId) {
    // New layout: demos/{demoId}/slides/slide01.cs ...
    const prefix = `demos/${demoId}/slides/`;
    codeFiles = files
      .filter((file) => file.name.startsWith(prefix) && file.name.endsWith(".cs"))
      .sort((a, b) => a.name.localeCompare(b.name));
  } else {
    // Legacy layout: code1.cs, code2.cs, ...
    codeFiles = files.filter((file) => file.name.startsWith("code"));
  }

  const contents = codeFiles.map(async (file): Promise<PublicFolderFile> => {
    const contents = await fetch(file.src);
    const text = await contents.text();

    return { filename: file.name, value: text };
  });

  return Promise.all(contents);
};

/**
 * Load the manifest.json for a demo.
 */
export const getManifest = async (demoId: string) => {
  const files = getStaticFiles();
  const manifestFile = files.find(
    (f) => f.name === `demos/${demoId}/manifest.json`
  );

  if (!manifestFile) {
    return null;
  }

  const res = await fetch(manifestFile.src);
  return res.json();
};
