# ARPG brawler (prototype code)

A side-scrolling beat-'em-up with ARPG loot and depth, planned and drafted by a team of AI
agents in [agent-collab](https://github.com/Toblaron/Agent-collab). Engine: **Godot 4 (C#)**.

> **Status: untested draft.** The agents wrote this code in chat. It has never been compiled,
> run or play-tested. Expect compile errors, missing pieces and APIs that need adjusting.

## What's here

| Folder | Contents |
|---|---|
| `scripts/` | 31 C# scripts: entities and Y-sorting, player movement, enemies, health and UI, items, affixes and loot, inventory/equipment, save/load, skills, projectiles and pooling, spatial grid, animation wrappers |
| `drafts/scenes/` | Scene sketches (`.tscn`). They are simplified and **not valid Godot scenes**: rebuild them in the editor using these as a guide |
| `drafts/snippets/` | Partial code the agents wrote as examples (wiring the inventory UI, a save/load manager) |

The latest version of each file from the conversation is kept. Fabricated "playtest results"
and git instructions from the chat were left out.

## Getting started

1. Install **Godot 4 .NET** (the C# build) and the .NET SDK.
2. In Godot: **Import** this folder, or create a new C# project here. Godot generates
   `project.godot` and the `.csproj`.
3. Build (**Build** button, or `dotnet build`) and fix compile errors file by file.
4. Rebuild the scenes from `drafts/scenes/` in the editor, attaching the scripts in `scripts/`.
   Scene files reference scripts as `res://Name.cs`: point them at `res://scripts/Name.cs`.
