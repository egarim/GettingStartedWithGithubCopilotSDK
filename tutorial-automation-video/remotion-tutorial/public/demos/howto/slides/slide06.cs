// Paso 5: Renderizar video final

// Renderizar un demo:
npx remotion render Demo01

// Renderizar todos:
for id in Demo01 Demo02 Demo03; do
  npx remotion render $id
done

// Output: out/Demo01.mp4
// 1920x1080, 30fps
