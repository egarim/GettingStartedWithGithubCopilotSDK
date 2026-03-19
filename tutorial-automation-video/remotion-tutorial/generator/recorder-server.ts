import { createServer } from "http";
import { readFileSync, writeFileSync, existsSync, mkdirSync, readdirSync, statSync } from "fs";
import { join, resolve } from "path";
import { execSync } from "child_process";

const PORT = parseInt(process.env.RECORDER_PORT ?? "3456", 10);
const REMOTION_PORT = parseInt(process.env.REMOTION_PORT ?? "3000", 10);

// Accept demos directory as CLI arg, env var, or default
const DEMOS_DIR_ARG = process.argv[2];
const PUBLIC_DIR = resolve(
  DEMOS_DIR_ARG ?? process.env.DEMOS_DIR ?? resolve(import.meta.dirname, "..", "public", "demos")
);
const RECORDER_HTML = resolve(import.meta.dirname, "recorder.html");

function getContentType(path: string): string {
  if (path.endsWith(".html")) return "text/html";
  if (path.endsWith(".json")) return "application/json";
  if (path.endsWith(".mp3")) return "audio/mpeg";
  if (path.endsWith(".webm")) return "audio/webm";
  if (path.endsWith(".cs")) return "text/plain";
  return "application/octet-stream";
}

const server = createServer(async (req, res) => {
  const url = new URL(req.url ?? "/", `http://localhost:${PORT}`);
  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
  res.setHeader("Access-Control-Allow-Headers", "Content-Type");

  if (req.method === "OPTIONS") {
    res.writeHead(200);
    res.end();
    return;
  }

  // Serve the recorder UI (inject config)
  if (url.pathname === "/" || url.pathname === "/recorder") {
    let html = readFileSync(RECORDER_HTML, "utf-8");
    html = html.replace("__REMOTION_PORT__", String(REMOTION_PORT));
    html = html.replace("__DEMOS_DIR__", PUBLIC_DIR.replace(/\\/g, "/"));
    res.writeHead(200, { "Content-Type": "text/html" });
    res.end(html);
    return;
  }

  // List available demos
  if (url.pathname === "/api/demos") {
    const demos = readdirSync(PUBLIC_DIR)
      .filter((d) => d.startsWith("demo"))
      .sort();
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify(demos));
    return;
  }

  // Get manifest for a demo
  if (url.pathname.match(/^\/api\/manifest\/demo\d+$/)) {
    const demoId = url.pathname.split("/").pop()!;
    const manifestPath = join(PUBLIC_DIR, demoId, "manifest.json");
    if (!existsSync(manifestPath)) {
      res.writeHead(404);
      res.end("Manifest not found");
      return;
    }
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(readFileSync(manifestPath, "utf-8"));
    return;
  }

  // Get slide code file
  if (url.pathname.match(/^\/api\/slide\/demo\d+\/slide\d+\.cs$/)) {
    const parts = url.pathname.split("/");
    const demoId = parts[3];
    const slideFile = parts[4];
    const filePath = join(PUBLIC_DIR, demoId, "slides", slideFile);
    if (!existsSync(filePath)) {
      res.writeHead(404);
      res.end("Slide not found");
      return;
    }
    res.writeHead(200, { "Content-Type": "text/plain" });
    res.end(readFileSync(filePath, "utf-8"));
    return;
  }

  // Check if audio exists for a slide
  if (url.pathname.match(/^\/api\/audio-exists\/demo\d+\/slide\d+\.mp3$/)) {
    const parts = url.pathname.split("/");
    const demoId = parts[3];
    const audioFile = parts[4];
    const filePath = join(PUBLIC_DIR, demoId, "audio", audioFile);
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify({ exists: existsSync(filePath) }));
    return;
  }

  // Serve existing audio
  if (url.pathname.match(/^\/audio\/demo\d+\/slide\d+\.mp3$/)) {
    const parts = url.pathname.split("/");
    const demoId = parts[2];
    const audioFile = parts[3];
    const filePath = join(PUBLIC_DIR, demoId, "audio", audioFile);
    if (!existsSync(filePath)) {
      res.writeHead(404);
      res.end("Audio not found");
      return;
    }
    res.writeHead(200, { "Content-Type": "audio/mpeg" });
    res.end(readFileSync(filePath));
    return;
  }

  // Upload recorded audio
  if (req.method === "POST" && url.pathname.match(/^\/api\/upload\/demo\d+\/slide\d+$/)) {
    const parts = url.pathname.split("/");
    const demoId = parts[3];
    const slideName = parts[4];
    const audioDir = join(PUBLIC_DIR, demoId, "audio");
    mkdirSync(audioDir, { recursive: true });

    const chunks: Buffer[] = [];
    req.on("data", (chunk) => chunks.push(chunk));
    req.on("end", () => {
      const buffer = Buffer.concat(chunks);
      // Save as webm first (browser native format)
      const webmPath = join(audioDir, `${slideName}.webm`);
      writeFileSync(webmPath, buffer);

      // Also save as mp3 name (Remotion will use this)
      // For now save the webm and the user can convert, or we handle it
      const mp3Path = join(audioDir, `${slideName}.mp3`);

      // Save webm directly — Remotion handles webm audio fine
      // Also copy as .mp3 extension so the manifest references work
      writeFileSync(mp3Path, buffer);
      console.log(`  Saved: ${webmPath} + ${mp3Path}`);

      res.writeHead(200, { "Content-Type": "application/json" });
      res.end(JSON.stringify({ ok: true, path: mp3Path }));
    });
    return;
  }

  // Re-sync durations: re-measure audio files and update manifest
  if (req.method === "POST" && url.pathname.match(/^\/api\/resync\/demo\d+$/)) {
    const demoId = url.pathname.split("/").pop()!;
    const demoNumber = demoId.replace("demo", "");
    const manifestPath = join(PUBLIC_DIR, demoId, "manifest.json");

    if (!existsSync(manifestPath)) {
      res.writeHead(404, { "Content-Type": "application/json" });
      res.end(JSON.stringify({ ok: false, error: "Manifest not found" }));
      return;
    }

    try {
      // Read current manifest
      const manifest = JSON.parse(readFileSync(manifestPath, "utf-8"));
      const audioDir = join(PUBLIC_DIR, demoId, "audio");
      let changed = false;

      const mm = await import("music-metadata");

      for (const slide of manifest.slides) {
        // Prefer .webm for duration (browser recordings have correct metadata)
        // The .mp3 from recorder is actually webm bytes with wrong extension
        const baseName = slide.audioFile.replace(/\.[^.]+$/, "");
        const webmPath = join(audioDir, `${baseName}.webm`);
        const mp3Path = join(audioDir, slide.audioFile);

        const measurePath = existsSync(webmPath) ? webmPath : mp3Path;
        if (!existsSync(measurePath)) continue;

        let durationMs: number | null = null;

        try {
          const metadata = await mm.parseFile(measurePath);
          if (metadata.format.duration && metadata.format.duration > 0) {
            durationMs = metadata.format.duration * 1000;
          }
        } catch {}

        // Fallback: try the other file if first didn't work
        if (durationMs === null && measurePath === mp3Path && existsSync(webmPath)) {
          try {
            const metadata = await mm.parseFile(webmPath);
            if (metadata.format.duration && metadata.format.duration > 0) {
              durationMs = metadata.format.duration * 1000;
            }
          } catch {}
        }

        // Last resort: file size estimate
        if (durationMs === null) {
          const stats = statSync(measurePath);
          durationMs = (stats.size / 4000) * 1000; // webm opus ~4kB/s
        }

        const FPS = 30;
        const PADDING = 2;
        const newFrames = Math.ceil((durationMs / 1000) * FPS) + PADDING * FPS;

        if (newFrames !== slide.durationFrames) {
          slide.durationFrames = newFrames;
          changed = true;
        }
      }

      if (changed) {
        manifest.totalFrames = manifest.slides.reduce(
          (sum: number, s: { durationFrames: number }) => sum + s.durationFrames, 0
        );
        writeFileSync(manifestPath, JSON.stringify(manifest, null, 2), "utf-8");
      }

      console.log(`  Re-synced ${demoId}: ${manifest.totalFrames} frames (~${Math.round(manifest.totalFrames / 30)}s)`);
      res.writeHead(200, { "Content-Type": "application/json" });
      res.end(JSON.stringify({
        ok: true,
        totalFrames: manifest.totalFrames,
        totalSeconds: Math.round(manifest.totalFrames / 30),
        changed,
      }));
    } catch (err) {
      res.writeHead(500, { "Content-Type": "application/json" });
      res.end(JSON.stringify({ ok: false, error: (err as Error).message }));
    }
    return;
  }

  // Trigger GitHub Actions render workflow
  if (req.method === "POST" && url.pathname === "/api/render") {
    const chunks: Buffer[] = [];
    req.on("data", (chunk) => chunks.push(chunk));
    req.on("end", async () => {
      try {
        const body = JSON.parse(Buffer.concat(chunks).toString());
        const demo = body.demo ?? "all";
        const token = body.token;

        if (!token) {
          res.writeHead(400, { "Content-Type": "application/json" });
          res.end(JSON.stringify({ ok: false, error: "GitHub token required" }));
          return;
        }

        // Dispatch the workflow via GitHub API
        const response = await fetch(
          "https://api.github.com/repos/egarim/GettingStartedWithGithubCopilotSDK/actions/workflows/render-videos.yml/dispatches",
          {
            method: "POST",
            headers: {
              Authorization: `Bearer ${token}`,
              Accept: "application/vnd.github.v3+json",
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              ref: "master",
              inputs: { demo },
            }),
          }
        );

        if (response.ok || response.status === 204) {
          console.log(`  Render dispatched: ${demo}`);
          res.writeHead(200, { "Content-Type": "application/json" });
          res.end(JSON.stringify({ ok: true, demo }));
        } else {
          const err = await response.text();
          res.writeHead(response.status, { "Content-Type": "application/json" });
          res.end(JSON.stringify({ ok: false, error: err }));
        }
      } catch (err) {
        res.writeHead(500, { "Content-Type": "application/json" });
        res.end(JSON.stringify({ ok: false, error: (err as Error).message }));
      }
    });
    return;
  }

  // Proxy Remotion Studio (strips CSP header so iframe works)
  if (url.pathname.startsWith("/remotion/") || url.pathname === "/remotion") {
    const remotionPath = url.pathname.replace("/remotion", "") || "/";
    const remotionUrl = `http://localhost:${REMOTION_PORT}${remotionPath}${url.search}`;
    try {
      const { default: http } = await import("http");
      const proxyReq = http.request(remotionUrl, (proxyRes) => {
        const headers = { ...proxyRes.headers };
        // Remove CSP so iframe embedding works
        delete headers["content-security-policy"];
        delete headers["x-frame-options"];
        res.writeHead(proxyRes.statusCode ?? 200, headers);
        proxyRes.pipe(res);
      });
      proxyReq.on("error", () => {
        res.writeHead(502, { "Content-Type": "text/plain" });
        res.end("Remotion Studio not running on port " + REMOTION_PORT);
      });
      req.pipe(proxyReq);
    } catch {
      res.writeHead(502);
      res.end("Proxy error");
    }
    return;
  }

  res.writeHead(404);
  res.end("Not found");
});

server.listen(PORT, () => {
  console.log(`\n  Audio Recorder Studio`);
  console.log(`  http://localhost:${PORT}\n`);
  console.log(`  Demos dir:    ${PUBLIC_DIR}`);
  console.log(`  Remotion:     http://localhost:${REMOTION_PORT}`);
  console.log(`\n  Usage: npx tsx recorder-server.ts [demos-dir]`);
  console.log(`  Env:   DEMOS_DIR, REMOTION_PORT, RECORDER_PORT\n`);
});
