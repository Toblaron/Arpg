# Løvstakken – notes for Claude

Godot 4.7.2 .NET (C#) belt-scrolling beat-'em-up (Final Fight / Turtles in Time style — NOT a single-screen arena) with D2/PoE-style loot and playable classes. See README.md for the game design.

## Tools on this machine

- Godot executable: `$GODOT4` (user env var; GUI exe). Console variant with the same path but `_console.exe` gives clean stdout.
- `godot` / `godot_console` on PATH are `.cmd` shims in `%LOCALAPPDATA%\Microsoft\WinGet\Links` (the winget symlinks broke .NET assembly loading).
- .NET 8 SDK. Solution: `Arpg.sln` (game `Arpg.csproj` + `tests/LootSim`).

## Build and verify — do this after every change

1. `dotnet build Arpg.csproj` — must be 0 errors.
2. `"$GODOT4" --headless --path . -s res://tests/smoke_test.gd` — plays the whole game headless (~2 min): combat, every enemy type, loot, kroner, the stage and fight zones, the boss, Matkroken and stage 2. Exit code 0 = all PASS. Add a check here when you add a gameplay feature. A GDScript error inside the test is caught by a 240 s watchdog.
3. `dotnet run --project tests/LootSim` — after touching ItemCatalog/ItemGenerator/ItemInstance/Stats.
4. After adding or renaming scenes/scripts outside the editor: `"$GODOT4" --headless --path . --import` so Godot generates `.uid` files.

## Godot MCP server (`godot`, configured in .mcp.json)

Tools: run_project, get_debug_output, stop_project, launch_editor, create_scene, add_node, save_scene, load_sprite, get_project_info, get_uid, update_project_uids.

- **Always pass `projectPath` as `C:/ai/game` with forward slashes.** Backslashes are stripped by the server (`C:aigame`) and every call fails.
- `run_project` opens a real game window on the user's screen; call `stop_project` when done. Use the headless smoke test for automated checks instead.

## Scene conventions

- Scenes live in `scenes/`: `Player.tscn`, `Enemy.tscn`, `Level.tscn` (main scene: `Backdrop`, player, `Camera2D` (zoom 2), `WaveSpawner`, `StageDirector`, `UI` CanvasLayer).
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
- `WaveSpawner.cs` spawns all enemies when the `StageDirector` calls `StartEncounter(waves)`; they enter from the left/right screen edges inside the street strip. Unlock table `Pool`; `Spawn(typeId, at)` for scripted spawns. The level has no hand-placed enemies. `WaveUI.cs` shows the counter, banners and GO arrow.
- `EnemyLook.cs`: one `Draw<Type>()` per enemy on a shared body frame; front arm swings via `ArmAngle`. Skin tone is random per enemy, never tied to type.
- `EnemyProjectile.cs`: Drug Dealer bottles (group "EnemyProjectile").
- Adding an enemy: entry in `EnemyType.All`, a case in `EnemyLook._Draw`, a row in `WaveSpawner.Pool`, the id in `ENEMY_TYPES` in `tests/smoke_test.gd`.

## Stage (belt scroller)

- `scripts/Stage/StageDirector.cs`: camera follows the player right only; `EncounterAt` x positions lock the screen and start `EncounterWaves`; `StageCleared` at `StageLength`. `WarpTo(x)` jumps ahead (tests/debug) — wait ~3 physics frames after it before checking `Locked`.
- `PlayBounds` (static, same file): walkable strip `FloorTop..FloorBottom` (sidewalk + road) and the screen's left/right edges. Player is clamped to both, enemies to depth only. Anything that moves characters must respect it.
- World units: the camera zooms 2x, so the screen shows 576x324 world px. Design screen 1152x648 (project setting, stretch `canvas_items`). Use `PlayBounds.ScreenSize`, not `GetViewportRect()` (the headless/window size can differ).
- `StreetBackdrop.cs`: sky (fixed), Løvstakken mountain (parallax 0.15), street layer — drawn at 0.5 scale in 1152x648 design units.

## Boss, kroner, Matkroken

- Boss = `EnemyType` `pantelaaner` (`EnemyBehaviour.Boss`, `Size` 1.7) handled in `Enemy.cs` (`Boss()`, `Slam()`, backup via `WaveSpawner.SpawnAtEdge` at `SummonAt` health fractions). Spawned by `StageDirector.StartBoss()` at `BossAt` after the last encounter; group "Boss"; `BossUI` shows its bar. Killing it → `ClearStage()`.
- `StageDirector.StartStage(n)` restarts the street as stage n (clears enemies/items/kroner, player to `PlayerStart`); wave count carries on.
- Kroner: `EnemyType.KronerMin/Max` → `KronerPickup.Drop` on death (group "Kroner", magnet pickup) → `Player.AddKroner` / `TrySpend`, signal `KronerChanged`. `KronerUI` shows it.
- Matkroken: `scripts/Shop/ShopCatalog.cs` (items + `PlayerUpgrades`), `ShopUI.cs` (opens `OpenDelay` s after `StageCleared`, pauses the tree, `Buy(id)`, `Continue()`). Upgrade effects are applied in `Player` (max health, damage, speed, ability cooldown, magic find). From GDScript use `player.call("UpgradeLevel", id)`.
- One Godot script class per file when the script is attached in a scene (file name = class name); helper classes created with `new` can share a file.

## Input actions (project.godot)

Movement: built-in `ui_left/right/up/down` (arrows + WASD). `attack` = J, `ability` = K (class ability), `inventory` = I (toggles the hidden inventory). Add new actions through a headless GDScript that edits `ProjectSettings` and calls `ProjectSettings.save()`, rather than hand-writing InputEvent objects.
