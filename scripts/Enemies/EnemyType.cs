// EnemyType.cs – the enemy roster: stats, fighting style and loot for each kind of enemy.
// Add a type: add an entry to All (and its look to EnemyLook). The Enemy node's TypeId picks one.
using System.Collections.Generic;

public enum EnemyBehaviour
{
    /// <summary>Walks straight at the player and hits in melee range.</summary>
    Melee,
    /// <summary>Twitchy zig-zag approach, hard to pin down.</summary>
    Erratic,
    /// <summary>Keeps its distance and throws bottles.</summary>
    Ranged,
    /// <summary>Winds up, then dashes through the player.</summary>
    Charger,
    /// <summary>End-of-stage boss: heavy swings, a telegraphed ground slam, calls for backup when hurt.</summary>
    Boss,
}

public sealed record EnemyType(
    string Id,
    string DisplayName,
    float MaxHealth,
    float Speed,
    float Damage,
    float AttackCooldown,
    EnemyBehaviour Behaviour,
    float AttackRange = 28f,
    float StunTaken = 1f,        // multiplier on stun time and knockback taken (Thug shrugs it off)
    int LootLevelBonus = 0,      // added to the item level of its drops
    int MaxDrops = 2,
    float DropChance = 0.6f,
    int KronerMin = 5,           // Norwegian kroner dropped on death (random in range)
    int KronerMax = 15,
    float Size = 1f)             // drawn and collision scale (the boss is big)
{
    public static readonly IReadOnlyDictionary<string, EnemyType> All = new Dictionary<string, EnemyType>
    {
        ["street_hustler"] = new("street_hustler", "Street Hustler", 22f, 150f, 5f, 0.7f, EnemyBehaviour.Melee, KronerMin: 8, KronerMax: 20),
        ["skank"] = new("skank", "Skank", 26f, 115f, 7f, 0.9f, EnemyBehaviour.Melee, KronerMin: 6, KronerMax: 16),
        ["thug"] = new("thug", "Thug", 60f, 65f, 14f, 1.4f, EnemyBehaviour.Melee, AttackRange: 32f, StunTaken: 0.5f, KronerMin: 15, KronerMax: 30),
        ["playa"] = new("playa", "Playa", 34f, 120f, 6f, 0.55f, EnemyBehaviour.Melee, KronerMin: 12, KronerMax: 28),
        ["drug_dealer"] = new("drug_dealer", "Drug Dealer", 26f, 95f, 8f, 2.0f, EnemyBehaviour.Ranged, LootLevelBonus: 1, KronerMin: 25, KronerMax: 45),
        ["baller"] = new("baller", "Baller", 45f, 80f, 8f, 1.1f, EnemyBehaviour.Melee, LootLevelBonus: 3, MaxDrops: 4, DropChance: 0.9f, KronerMin: 80, KronerMax: 150),
        ["g"] = new("g", "G", 40f, 95f, 12f, 1.6f, EnemyBehaviour.Charger, KronerMin: 15, KronerMax: 30),
        ["dope_fiend"] = new("dope_fiend", "Dope Fiend", 18f, 140f, 6f, 0.8f, EnemyBehaviour.Erratic, KronerMin: 1, KronerMax: 5),
        // The stage boss (not in the wave pool): spawned by the StageDirector at the end of the street.
        ["pantelaaner"] = new("pantelaaner", "Pantelåneren", 420f, 70f, 16f, 1.3f, EnemyBehaviour.Boss, AttackRange: 44f,
            StunTaken: 0.15f, LootLevelBonus: 4, MaxDrops: 5, DropChance: 1f, KronerMin: 400, KronerMax: 600, Size: 1.7f),
    };

    public const string BossId = "pantelaaner";

    public static EnemyType Get(string id) =>
        All.TryGetValue(id ?? "", out var type) ? type : All["street_hustler"];
}
