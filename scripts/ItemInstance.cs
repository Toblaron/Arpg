// ItemInstance.cs – one dropped item: its base, rarity, level and rolled affixes.
// Plain data (no Godot types) so it saves to JSON as-is and never loses its rolls.
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class RolledStat
{
    public StatKey Stat { get; set; }
    public float Value { get; set; }
}

public sealed class RolledAffix
{
    public string AffixId { get; set; } = "";
    public AffixKind Kind { get; set; }
    public int Tier { get; set; }
    public string Name { get; set; } = "";   // "Vicious", "of the Drake"
    public StatKey Stat { get; set; }
    public float Value { get; set; }
}

public sealed class ItemInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string BaseId { get; set; } = "";
    public string Name { get; set; } = "";
    public Rarity Rarity { get; set; }
    public int ItemLevel { get; set; }
    public List<RolledStat> Implicits { get; set; } = new();
    public List<RolledAffix> Affixes { get; set; } = new();

    public ItemBase Base => ItemCatalog.GetBase(BaseId);
    public ItemSlot Slot => Base?.Slot ?? ItemSlot.Ring;
    public string BaseName => Base?.Name ?? BaseId;
    public IEnumerable<RolledAffix> Prefixes => Affixes.Where(a => a.Kind == AffixKind.Prefix);
    public IEnumerable<RolledAffix> Suffixes => Affixes.Where(a => a.Kind == AffixKind.Suffix);

    /// <summary>
    /// Final stats this item gives. "% increased Armor" scales the item's own armor (local, like
    /// PoE); other stats are summed and handed to the character.
    /// </summary>
    public Stats GetTotalStats()
    {
        var total = new Stats();
        foreach (var s in Implicits) total.Add(s.Stat, s.Value);
        foreach (var a in Affixes) total.Add(a.Stat, a.Value);
        if (total[StatKey.IncreasedArmor] != 0f)
        {
            total[StatKey.Armor] = Mathf.Round(total[StatKey.Armor] * (1f + total[StatKey.IncreasedArmor] / 100f));
            total[StatKey.IncreasedArmor] = 0f;
        }
        return total;
    }

    /// <summary>Name colours as in D2/PoE: white, blue, yellow, orange.</summary>
    public Color RarityColor => Rarity switch
    {
        Rarity.Magic => new Color(0.53f, 0.53f, 1f),
        Rarity.Rare => new Color(1f, 1f, 0.47f),
        Rarity.Unique => new Color(0.69f, 0.38f, 0.15f),
        _ => new Color(0.9f, 0.9f, 0.9f),
    };

    /// <summary>Tooltip text: name, base, item level, implicits, then affix lines.</summary>
    public string Describe()
    {
        var lines = new List<string> { Name };
        if (Rarity == Rarity.Rare || Rarity == Rarity.Unique) lines.Add(BaseName);
        lines.Add($"{Slot} · Item Level {ItemLevel}");
        if (Implicits.Count > 0)
        {
            lines.Add("──────────");
            lines.AddRange(Implicits.Select(s => StatText.Format(s.Stat, s.Value)));
        }
        if (Affixes.Count > 0)
        {
            lines.Add("──────────");
            lines.AddRange(Prefixes.Concat(Suffixes).Select(a => StatText.Format(a.Stat, a.Value)));
        }
        return string.Join("\n", lines);
    }
}
