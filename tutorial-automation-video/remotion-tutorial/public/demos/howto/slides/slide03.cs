// Paso 2: Generar slides y audio

// Todos los demos:
npx tsx cli.ts --all

// Un demo especifico:
npx tsx cli.ts --demo 01

// Solo slides (sin audio):
npx tsx cli.ts --all --skip-audio

// Preview sin generar:
npx tsx cli.ts --demo 03 --dry-run
