// ShopCatalog.cs – what Matkroken sells and what each purchase does to the player.
// Upgrades are permanent and stack; each level costs more. Food (Pølse i lompe) is a one-off.
using System.Collections.Generic;
using Godot;

public sealed record ShopItem(
    string Id,
    string Name,
    string Description,
    int BasePrice,
    float PriceGrowth = 1.6f,   // price × this per level already bought
    int MaxLevel = 10,
    bool Consumable = false)
{
    public int PriceAt(int level) => Mathf.RoundToInt(BasePrice * Mathf.Pow(PriceGrowth, level) / 10f) * 10;
}

public static class ShopCatalog
{
    public static readonly ShopItem[] Items =
    {
        new("polse", "Pølse i lompe", "Heal to full", 60, PriceGrowth: 1f, Consumable: true),
        new("brunost", "Brunost", "+20 max health", 150),
        new("fiskeboller", "Fiskeboller", "+15% damage", 200),
        new("energidrikk", "Energidrikk", "+8% movement speed", 150, MaxLevel: 5),
        new("kaffe", "Svart kaffe", "Class ability cooldown −15%", 180, MaxLevel: 4),
        new("skrapelodd", "Skrapelodd", "+25% magic find", 120),
    };

    public static ShopItem Get(string id)
    {
        foreach (var item in Items) if (item.Id == id) return item;
        return null;
    }
}

/// <summary>A player's Matkroken upgrades and the bonuses they add up to.</summary>
public sealed class PlayerUpgrades
{
    private readonly Dictionary<string, int> _levels = new();

    public int Level(string id) => _levels.TryGetValue(id, out int l) ? l : 0;
    public void Add(string id) => _levels[id] = Level(id) + 1;

    public float MaxHealthBonus => 20f * Level("brunost");
    public float DamageMultiplier => 1f + 0.15f * Level("fiskeboller");
    public float SpeedMultiplier => 1f + 0.08f * Level("energidrikk");
    public float AbilityCooldownMultiplier => Mathf.Pow(0.85f, Level("kaffe"));
    public float MagicFindBonus => 25f * Level("skrapelodd");
}
