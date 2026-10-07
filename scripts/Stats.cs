// Stats.cs – the stat keys items and characters use, and a bag of values you can add together.
using System.Collections.Generic;
using System.Linq;

public enum StatKey
{
    // Offence
    AddedDamage,        // flat damage added to attacks
    IncreasedDamage,    // % increased damage
    AttackSpeed,        // % increased attack speed
    CritChance,         // % chance to critically strike
    LifeOnHit,          // life gained per hit
    // Defence
    Health,             // flat maximum life
    Armor,              // flat armor
    IncreasedArmor,     // % increased armor (applies to the item's own armor)
    FireResistance,     // %
    ColdResistance,     // %
    LightningResistance,// %
    // Utility
    MoveSpeed,          // % increased movement speed
    MagicFind,          // % increased rarity of items found
}

/// <summary>A set of stat values. Missing stats read as 0; adding two Stats sums each key.</summary>
public class Stats
{
    private readonly Dictionary<StatKey, float> _values = new();

    public float this[StatKey key]
    {
        get => _values.TryGetValue(key, out var v) ? v : 0f;
        set => _values[key] = value;
    }

    public void Add(StatKey key, float amount) => this[key] = this[key] + amount;

    public IEnumerable<KeyValuePair<StatKey, float>> NonZero => _values.Where(kv => kv.Value != 0f);

    public static Stats operator +(Stats a, Stats b)
    {
        var sum = new Stats();
        foreach (var kv in a.NonZero) sum.Add(kv.Key, kv.Value);
        foreach (var kv in b.NonZero) sum.Add(kv.Key, kv.Value);
        return sum;
    }

    public override string ToString() =>
        string.Join("\n", NonZero.OrderBy(kv => kv.Key).Select(kv => StatText.Format(kv.Key, kv.Value)));
}

/// <summary>Human-readable stat lines, e.g. "+12 to maximum Life", "18% increased Attack Speed".</summary>
public static class StatText
{
    public static string Format(StatKey key, float v)
    {
        string n = v % 1f == 0f ? v.ToString("0") : v.ToString("0.#");
        return key switch
        {
            StatKey.AddedDamage => $"Adds {n} Damage to Attacks",
            StatKey.IncreasedDamage => $"{n}% increased Damage",
            StatKey.AttackSpeed => $"{n}% increased Attack Speed",
            StatKey.CritChance => $"+{n}% Critical Strike Chance",
            StatKey.LifeOnHit => $"Gain {n} Life per Hit",
            StatKey.Health => $"+{n} to maximum Life",
            StatKey.Armor => $"+{n} to Armor",
            StatKey.IncreasedArmor => $"{n}% increased Armor",
            StatKey.FireResistance => $"+{n}% to Fire Resistance",
            StatKey.ColdResistance => $"+{n}% to Cold Resistance",
            StatKey.LightningResistance => $"+{n}% to Lightning Resistance",
            StatKey.MoveSpeed => $"{n}% increased Movement Speed",
            StatKey.MagicFind => $"{n}% increased Rarity of Items found",
            _ => $"{key}: {n}",
        };
    }
}
