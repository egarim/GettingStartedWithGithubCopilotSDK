# 08 – Skill Loading & Configuration

## Concept / Concepto

A **Skill** is a set of instructions that modify the model's behavior for a session. Skills are defined as `SKILL.md` files in subdirectories. When loaded, the skill's instructions are added to the model's system prompt, changing how it responds. You can load multiple skills, disable specific ones, and create custom skills at runtime.

Skills are the way to inject domain-specific knowledge, coding standards, or behavioral rules into the Copilot model without modifying the system prompt directly.

---

## What You'll Learn / Lo que aprenderás

| # | Topic | Description |
|---|-------|-------------|
| 1 | **Load & Apply Skill** | Use `SkillDirectories` to load a skill from disk. Verify the skill affects responses (marker check). |
| 2 | **Disable Skill** | Use `DisabledSkills` to exclude a specific skill by name, even if its directory is loaded. |
| 3 | **Baseline Comparison** | A session with no skills — no marker appears, confirming the skill was the source of the behavior. |

---

## SKILL.md File Format

A skill is defined by a `SKILL.md` file inside a named subdirectory:

```
📁 skills-directory/
  📁 my-skill/
    📄 SKILL.md
```

The file uses **YAML frontmatter** followed by **Markdown** instructions:

```markdown
---
name: my-skill
description: A brief description of what this skill does
---

# Skill Instructions

Your detailed instructions go here. The model will follow these
instructions for every response in sessions where this skill is loaded.
```

---

## Key APIs

```csharp
// Load skills from a directory
var session = await client.CreateSessionAsync(new SessionConfig
{
    SkillDirectories = ["/path/to/skills-directory"]
});

// Load skills but disable specific ones
var session = await client.CreateSessionAsync(new SessionConfig
{
    SkillDirectories = ["/path/to/skills-directory"],
    DisabledSkills = ["my-skill"]  // skill name from YAML frontmatter
});
```

---

## How Skills Work

```
  📁 skills-dir/
    📁 demo-skill/
      📄 SKILL.md (contains marker instruction)
         │
         ▼
  ┌─── SessionConfig ───────────────────┐
  │  SkillDirectories: [skills-dir]     │
  │  DisabledSkills: []                 │
  └──────────┬──────────────────────────┘
             │
             ▼
  SDK reads SKILL.md → injects into system prompt
             │
             ▼
  Model follows skill instructions in every response
  (e.g., includes "PINEAPPLE_COCONUT_42" marker)
```

---

## Verification Pattern

The demo uses a **marker verification** technique to prove a skill was applied:

1. Skill instructs: "Include `PINEAPPLE_COCONUT_42` in every response"
2. **With skill loaded** → response contains marker ✅
3. **With skill disabled** → response does NOT contain marker ✅
4. **Without skill** → response does NOT contain marker ✅

---

## Interactive Mode

Press **Enter** to create your own skill at runtime. You type a custom instruction (e.g., "Always respond in haiku format"), and the demo creates a `SKILL.md` file and starts a chat session with it active.

---

## Demo Recording

This demo includes 5 step files in `steps/` that build up `Program.cs` incrementally. Use with `record-demo.sh`:

```bash
./record-demo.sh 08.SkillsDemo
```

## Running / Ejecución

```bash
dotnet run --project 08.SkillsDemo
```
