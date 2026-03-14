#!/usr/bin/env bash
# ──────────────────────────────────────────────────────────────────────────────
# record-demo.sh — Automate demo recording for Copilot SDK presentations
#
# Usage:
#   ./record-demo.sh <demo-folder> [output-dir]
#
# Examples:
#   ./record-demo.sh 02.SessionDemo
#   ./record-demo.sh 03.ToolsDemo ~/Desktop/MyRecording
#
# What it does:
#   1. Creates a fresh console project in [output-dir] (default: ~/CopilotDemo)
#   2. Updates the .csproj with the right packages
#   3. Loads step files from <demo-folder>/steps/
#   4. On each Enter press, copies the next step to Program.cs
#   5. Optionally runs 'dotnet run' when you press 'r'
#
# Controls:
#   Enter  → advance to next step (overwrites Program.cs)
#   r      → run the project (dotnet run)
#   b      → run the project in background and continue
#   d      → show diff from previous step
#   s      → show current step info
#   q      → quit
# ──────────────────────────────────────────────────────────────────────────────
set -euo pipefail

REPO_DIR="$(cd "$(dirname "$0")" && pwd)"
DEMO_FOLDER="${1:?Usage: $0 <demo-folder> [output-dir]}"
OUTPUT_DIR="${2:-$HOME/CopilotDemo}"

DEMO_PATH="$REPO_DIR/$DEMO_FOLDER"
STEPS_DIR="$DEMO_PATH/steps"
CSPROJ_SRC="$DEMO_PATH"/*.csproj

# ── Validate ──────────────────────────────────────────────────────────────────
if [ ! -d "$STEPS_DIR" ]; then
    echo "ERROR: No steps/ directory found in $DEMO_PATH"
    echo "Run 'generate-steps.sh' first to create step files."
    exit 1
fi

STEP_FILES=($(ls "$STEPS_DIR"/step*.cs 2>/dev/null | sort))
TOTAL_STEPS=${#STEP_FILES[@]}

if [ "$TOTAL_STEPS" -eq 0 ]; then
    echo "ERROR: No step*.cs files found in $STEPS_DIR"
    exit 1
fi

# ── Read csproj template ─────────────────────────────────────────────────────
CSPROJ_FILE=$(ls "$DEMO_PATH"/*.csproj 2>/dev/null | head -1)
if [ -z "$CSPROJ_FILE" ]; then
    echo "ERROR: No .csproj found in $DEMO_PATH"
    exit 1
fi

PROJECT_NAME=$(basename "$CSPROJ_FILE" .csproj)

# ── Create project ───────────────────────────────────────────────────────────
echo "================================================================"
echo "  Recording setup: $DEMO_FOLDER"
echo "  Output directory: $OUTPUT_DIR"
echo "  Steps available: $TOTAL_STEPS"
echo "================================================================"
echo ""

if [ -d "$OUTPUT_DIR" ]; then
    echo "Output directory already exists: $OUTPUT_DIR"
    read -p "Delete and recreate? (y/n): " CONFIRM
    if [ "$CONFIRM" = "y" ]; then
        rm -rf "$OUTPUT_DIR"
    else
        echo "Aborting."
        exit 0
    fi
fi

mkdir -p "$OUTPUT_DIR"

echo "  Creating project..."
dotnet new console -n "$PROJECT_NAME" -o "$OUTPUT_DIR" --force > /dev/null 2>&1

# Copy the csproj (overwrite the generated one)
cp "$CSPROJ_FILE" "$OUTPUT_DIR/$PROJECT_NAME.csproj"

echo "  Restoring packages..."
(cd "$OUTPUT_DIR" && dotnet restore > /dev/null 2>&1)

echo "  Project ready at: $OUTPUT_DIR"
echo ""

# ── Step through ─────────────────────────────────────────────────────────────
CURRENT_STEP=0
PREV_STEP_FILE=""

show_status() {
    echo ""
    echo "────────────────────────────────────────────────────────────────"
    if [ "$CURRENT_STEP" -lt "$TOTAL_STEPS" ]; then
        echo "  Step $((CURRENT_STEP + 1))/$TOTAL_STEPS ready"
        echo "  File: $(basename "${STEP_FILES[$CURRENT_STEP]}")"
    else
        echo "  All $TOTAL_STEPS steps completed!"
    fi
    echo "────────────────────────────────────────────────────────────────"
    echo "  [Enter] next step  [r] run  [d] diff  [s] status  [q] quit"
    echo "────────────────────────────────────────────────────────────────"
}

advance_step() {
    if [ "$CURRENT_STEP" -ge "$TOTAL_STEPS" ]; then
        echo "  No more steps!"
        return
    fi

    local step_file="${STEP_FILES[$CURRENT_STEP]}"
    PREV_STEP_FILE="$OUTPUT_DIR/Program.cs.prev"

    # Save previous for diff
    if [ -f "$OUTPUT_DIR/Program.cs" ]; then
        cp "$OUTPUT_DIR/Program.cs" "$PREV_STEP_FILE"
    fi

    # Copy step to Program.cs
    cp "$step_file" "$OUTPUT_DIR/Program.cs"

    echo ""
    echo "  >>> Step $((CURRENT_STEP + 1))/$TOTAL_STEPS applied: $(basename "$step_file")"

    CURRENT_STEP=$((CURRENT_STEP + 1))
}

run_project() {
    echo ""
    echo "  Running: dotnet run..."
    echo "  ──────────────────────────────────────────────────────────"
    (cd "$OUTPUT_DIR" && dotnet run)
    echo "  ──────────────────────────────────────────────────────────"
    echo "  Done."
}

show_diff() {
    if [ ! -f "$PREV_STEP_FILE" ]; then
        echo "  No previous step to diff against."
        return
    fi
    echo ""
    diff --color=always -u "$PREV_STEP_FILE" "$OUTPUT_DIR/Program.cs" || true
}

# ── Main loop ─────────────────────────────────────────────────────────────────
show_status

while true; do
    read -rsn1 KEY

    case "$KEY" in
        "")  # Enter
            advance_step
            show_status
            ;;
        r|R)
            run_project
            show_status
            ;;
        b|B)
            echo ""
            echo "  Running in background..."
            (cd "$OUTPUT_DIR" && dotnet run) &
            ;;
        d|D)
            show_diff
            ;;
        s|S)
            show_status
            ;;
        q|Q)
            echo ""
            echo "  Done! Project remains at: $OUTPUT_DIR"
            exit 0
            ;;
    esac
done
