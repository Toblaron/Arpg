# Løvstakken

A side-scrolling beat-'em-up with ARPG loot (prototype), in the style of Final Fight and Turtles in Time. Pick a class, walk the street, crack heads, grab loot.

## Classes

Each class has its own look and a unique ability (key **K**).

| Class | Look | Unique ability |
|---|---|---|
| **Pimp** | Dark violet zoot suit, matching feathered dapper hat, gold chains and rings, a huge pimp cane | **Bitch-Slap**: a huge backhand to the front. Triple damage, knocks enemies back and stuns them for 1.2 s. 4 s cooldown. |

## Enemies

Every enemy shows its name above its head. Skin tones are random per enemy.

| Enemy | Look | Fights by | HP | Notes |
|---|---|---|---|---|
| **Street Hustler** | Flat cap, leather jacket, a sleeve full of gold watches | Fast melee | 22 | Quick, weak hits |
| **Skank** | Teased blonde hair, gold hoops, leopard jacket, pink skirt, handbag | Melee | 26 | Handbag swings |
| **Thug** | Hood up, scowl, baseball bat | Slow, heavy melee | 60 | Hits for 14; takes half stun and knockback |
| **Playa** | White suit, open pink shirt, shades, a rose | Rapid melee | 34 | Attacks every 0.55 s |
| **Drug Dealer** | Olive puffer, backwards red cap, cross-body bag | Ranged | 26 | Keeps 150–230 px away and throws bottles |
| **Baller** | Fur coat, heavy gold chains, gold shades, fan of cash | Melee | 45 | Loot piñata: up to 4 drops at +3 item level |
| **G** | Tank top, tattoos, blue bandana, khakis | Charger | 40 | Glows orange as he winds up, then dashes through you |
| **Dope Fiend** | Hunched, ragged patched hoodie, torn jeans, twitchy | Erratic | 18 | Zig-zags in, hard to pin down |

### Waves

Enemies come in waves at each fight zone (see Stage). Wave 1 is 3 enemies and each wave adds one (up to 12), with
+8% enemy health per wave and better loot every second wave. Tougher types unlock as you go:

| Wave | New arrivals |
|---|---|
| 1 | Street Hustler, Skank, Dope Fiend |
| 2 | Playa |
| 3 | Thug, Drug Dealer |
| 4 | G |
| 5 | Baller (rare) |

Enemies live in `scripts/Enemies/`: stats in `EnemyType.cs`, looks in `EnemyLook.cs`, waves in `WaveSpawner.cs` (wave
sizes and timings are settings on the `WaveSpawner` node in `Level.tscn`).

## Stage

A belt-scrolling street brawler in the style of Final Fight and Turtles in Time. Stage 1 is a night-time street under
Løvstakken: walk right along the sidewalk and road (up/down moves in depth), the camera follows and never scrolls back.

- **Fight zones:** at four points the screen locks and enemies come in from both sides: 1, 2, 2 and 3 waves (8 in all).
  Beat them and a blinking **GO →** sends you on.
- **Stage clear:** reach the Løvstakkveien sign at the end of the street.
- The street, buildings, neon signs and the mountain (scrolling slower, for depth) are drawn in code in
  `scripts/Stage/StreetBackdrop.cs`. Fight zone positions and wave counts are settings on the `StageDirector` node.

## Controls

| Key | Action |
|---|---|
| Arrows / WASD | Move (up/down = depth along the street) |
| J | Attack |
| K | Class ability (Bitch-Slap) |
| I | Inventory (click an item to equip it) |

Classes live in `scripts/Classes/`. To add one: subclass `PlayerClass` (name, ability, cooldown, `UseAbility`), give it a
`ClassLook` for its appearance, and add its id to `PlayerClass.Create`. The player's class is the `ClassId` setting on
the Player node.

## About


A side-scrolling beat-'em-up with ARPG loot and depth: Golden Axe / Final Fight combat, Diablo /
Path of Exile items. Planned by AI agents in [agent-collab](https://github.com/Toblaron/Agent-collab),
then made to compile. Engine: **Godot 4.7 (.NET / C#)**.

> **Status:** compiles cleanly (0 errors, 0 warnings) against Godot.NET.Sdk 4.7.2 and the loot
> rules pass their simulation test. It has **not been run inside the Godot editor yet**: the
> scenes still need building (see below).

## Loot (D2 / PoE style)

Every drop is rolled at random from **item bases** plus **affixes**:

- **Bases** (`scripts/ItemCatalog.cs`): Short Sword, Grim Scythe, Chain Mail, Great Helm, Coral Ring
  and more. Each has a slot, a drop level (better bases only drop deeper in) and implicit stats
  rolled in a range (weapon damage, armor…).
- **Rarity**: Normal (white), Magic (blue), Rare (yellow), roughly 70 / 25 / 4% before magic find.
  Magic find boosts magic items fully and rares with diminishing returns, as in D2.
- **Affixes**: prefixes (damage, life, armor) and suffixes (attack speed, crit, resistances, life on
  hit, movement speed, magic find), each limited to the slots it suits. Magic items get 1-2 (at most
  1 prefix + 1 suffix), rares 3-6 (at most 3 + 3), and never two from the same family.
- **Tiers by item level**: `Heavy` → `Serrated` → `Vicious` → `Merciless`. Stronger tiers need a
  higher item level, which comes from the monster that dropped it (`LootDrop.MonsterLevel`).
- **Names**: magic items are named from their affixes (*Chain Mail of the Inuit*), rares get
  two random words (*Vortex Spike*, *Damnation Keep*).
- Items on the ground show their name in rarity colour; walk over them to pick up. Click an item in
  the inventory to equip it (swaps with what you wore). Saves keep every roll (`InventorySerializer`).

Add bases and affixes by editing the tables in `ItemCatalog.cs`. Check the rules still hold with:

```bash
dotnet run --project tests/LootSim
```

## Layout

| Path | Contents |
|---|---|
| `scripts/` | Game code: Player, Enemy, Health, YSort depth, items and loot, inventory, equipment, save/load, skills (fireball, slash), projectile pool, spatial grid, UI helpers |
| `tests/LootSim/` | Simulates 20,000 drops and checks every loot rule |
| `drafts/` | The agents' scene sketches and snippets, kept for reference (not valid Godot files) |

## Getting started

1. Install **Godot 4.7 .NET** and the .NET 8 SDK.
2. Open this folder in Godot (it has `project.godot` and `Arpg.csproj`) and press **Build**.
3. Build the scenes in the editor, using the expected children noted at the top of each script:
   - **Player** (`CharacterBody2D` + `Player.cs`): `CollisionShape2D`, `Area2D` "Hurtbox", `Health` "Health", `EquipmentComponent` "Equipment".
   - **Enemy** (`CharacterBody2D` + `Enemy.cs`): `CollisionShape2D`, `Health` "Health", `LootDrop` "LootDrop".
   - **UI**: a `CanvasLayer` with `InventoryGrid` (set its Tooltip and Equipment), a `Tooltip`, and `HealthUI` pointed at the player's Health.
4. Drop a few enemies in a level, kill them, and watch the loot.
