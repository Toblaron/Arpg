# ARPG brawler (prototype)

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
