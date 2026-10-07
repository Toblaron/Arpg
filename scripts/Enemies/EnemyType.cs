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
    float DropChance = 0.6f)
{
    public static readonly IReadOnlyDictionary<string, EnemyType> All = new Dictionary<string, EnemyType>
    {
        ["street_hustler"] = new("street_hustler", "Street Hustler", 22f, 150f, 5f, 0.7f, EnemyBehaviour.Melee),
        ["skank"] = new("skank", "Skank", 26f, 115f, 7f, 0.9f, EnemyBehaviour.Melee),
        ["thug"] = new("thug", "Thug", 60f, 65f, 14f, 1.4f, EnemyBehaviour.Melee, AttackRange: 32f, StunTaken: 0.5f),
        ["playa"] = new("playa", "Playa", 34f, 120f, 6f, 0.55f, EnemyBehaviour.Melee),
        ["drug_dealer"] = new("drug_dealer", "Drug Dealer", 26f, 95f, 8f, 2.0f, EnemyBehaviour.Ranged, LootLevelBonus: 1),
        ["baller"] = new("baller", "Baller", 45f, 80f, 8f, 1.1f, EnemyBehaviour.Melee, LootLevelBonus: 3, MaxDrops: 4, DropChance: 0.9f),
        ["g"] = new("g", "G", 40f, 95f, 12f, 1.6f, EnemyBehaviour.Charger),
        ["dope_fiend"] = new("dope_fiend", "Dope Fiend", 18f, 140f, 6f, 0.8f, EnemyBehaviour.Erratic),
    };

    public static EnemyType Get(string id) =>
        All.TryGetValue(id ?? "", out var type) ? type : All["street_hustler"];
}
