# Løvstakken – notes for Claude

Godot 4.7.2 .NET (C#) side-scrolling brawler with D2/PoE-style loot and playable classes. See README.md for the game design.

## Tools on this machine

- Godot executable: `$GODOT4` (user env var; GUI exe). Console variant with the same path but `_console.exe` gives clean stdout.
- `godot` / `godot_console` on PATH are `.cmd` shims in `%LOCALAPPDATA%\Microsoft\WinGet\Links` (the winget symlinks broke .NET assembly loading).
- .NET 8 SDK. Solution: `Arpg.sln` (game `Arpg.csproj` + `tests/LootSim`).

## Build and verify — do this after every change

1. `dotnet build Arpg.csproj` — must be 0 errors.
2. `"$GODOT4" --headless --path . -s res://tests/smoke_test.gd` — plays the level headless: attack kills enemies, loot drops, pickup, equip. Exit code 0 = all PASS. Add a check here when you add a gameplay feature.
3. `dotnet run --project tests/LootSim` — after touching ItemCatalog/ItemGenerator/ItemInstance/Stats.
4. After adding or renaming scenes/scripts outside the editor: `"$GODOT4" --headless --path . --import` so Godot generates `.uid` files.

## Godot MCP server (`godot`, configured in .mcp.json)

Tools: run_project, get_debug_output, stop_project, launch_editor, create_scene, add_node, save_scene, load_sprite, get_project_info, get_uid, update_project_uids.

- **Always pass `projectPath` as `C:/ai/game` with forward slashes.** Backslashes are stripped by the server (`C:aigame`) and every call fails.
- `run_project` opens a real game window on the user's screen; call `stop_project` when done. Use the headless smoke test for automated checks instead.

## Scene conventions

- Scenes live in `scenes/`: `Player.tscn`, `Enemy.tscn`, `Level.tscn` (main scene: floor, player, `WaveSpawner`, `UI` CanvasLayer).
- C# classes are not `[GlobalClass]`, so in a `.tscn` a scripted node is its base Godot type plus `script = ExtResource(...)` (e.g. Health = `Node` + Health.cs).
- Scripts look up children by exact name (`Health`, `Equipment`, `Hurtbox`, `LootDrop`, `Inventory`) — keep names.
- Node-typed `[Export]` properties are saved as `NodePath` with `node_paths=PackedStringArray(...)` on the node header (see `Level.tscn` UI).
- In `Level.tscn` the `UI` node must stay below `Player` so the player's Health is ready before HealthUI reads it.
- Editing `.tscn` as text is fine, but not while the same scene is open in the Godot editor (it overwrites on save). Ask the user to close it or reload.

## Classes

- `scripts/Classes/PlayerClass.cs`: base class + `PlayerClass.Create(id)` registry, and `ClassLook` (drawn character, flips with facing, `PlayAttack`/`PlayAbility` animations).
- Pimp (`PimpClass`, `PimpLook`, `SlapEffect`): ability Bitch-Slap. Characters are drawn in code with `_Draw()` (no sprite assets yet); the look replaces the placeholder `Body` ColorRect at runtime.
- `Player.Facing` is ±X; class abilities aim with it. `Enemy.Stun(seconds, knockback)` dazes and slides an enemy.
- To check visuals, run a short windowed (not headless) GDScript that saves `get_viewport().get_texture().get_image()` to PNG — it briefly opens a window on the user's screen.

## Enemies

- `scripts/Enemies/EnemyType.cs`: roster (stats, `EnemyBehaviour` Melee/Erratic/Ranged/Charger, loot bonuses), keyed by id. `Enemy.TypeId` (export) picks one.
- `WaveSpawner.cs` (node in `Level.tscn`) spawns all enemies: unlock table `Pool`, `Spawn(typeId, at)` for scripted spawns, `StartNextWave()`. The level has no hand-placed enemies. `WaveUI.cs` shows the counter/banners. Tests set `AutoStart = false` before adding the level to the tree.
- `EnemyLook.cs`: one `Draw<Type>()` per enemy on a shared body frame; front arm swings via `ArmAngle`. Skin tone is random per enemy, never tied to type.
- `EnemyProjectile.cs`: Drug Dealer bottles (group "EnemyProjectile").
- Adding an enemy: entry in `EnemyType.All`, a case in `EnemyLook._Draw`, a row in `WaveSpawner.Pool`, the id in `ENEMY_TYPES` in `tests/smoke_test.gd`.

## Input actions (project.godot)

Movement: built-in `ui_left/right/up/down` (arrows + WASD). `attack` = J, `ability` = K (class ability). Add new actions through a headless GDScript that edits `ProjectSettings` and calls `ProjectSettings.save()`, rather than hand-writing InputEvent objects.
