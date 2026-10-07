// ItemGenerator.cs – rolls random items from the catalogue, D2/PoE style.
//
//   item level ─► eligible bases (DropLevel ≤ ilvl) ─► weighted base pick
//   magic find ─► rarity (Normal / Magic / Rare)
//   rarity     ─► affix count: Magic 1-2 (max 1 prefix + 1 suffix), Rare 3-6 (max 3 + 3)
//   each affix ─► family not already on the item ─► tier allowed at this ilvl ─► value in range
using System;
using System.Collections.Generic;
using System.Linq;

public static class ItemGenerator
{
    // Base rarity weights before magic find (Unique is reserved for hand-made items later).
    public const float NormalWeight = 1000f, MagicWeight = 350f, RareWeight = 60f;

    public static ItemInstance Generate(int itemLevel, float magicFind = 0f, Random rng = null)
    {
        rng ??= Random.Shared;
        itemLevel = Math.Max(1, itemLevel);
        var itemBase = PickBase(itemLevel, rng);
        var item = new ItemInstance { BaseId = itemBase.Id, ItemLevel = itemLevel, Rarity = RollRarity(magicFind, rng) };

        foreach (var r in itemBase.Implicits)
            item.Implicits.Add(new RolledStat { Stat = r.Stat, Value = RollValue(r.Min, r.Max, rng) });

        var (min, max, maxEach) = item.Rarity switch
        {
            Rarity.Magic => (1, 2, 1),
            Rarity.Rare => (3, 6, 3),
            _ => (0, 0, 0),
        };
        int count = rng.Next(min, max + 1);
        for (int i = 0; i < count; i++)
        {
            var affix = RollAffix(item, itemBase.Slot, maxEach, rng);
            if (affix == null) break; // nothing left that can roll on this base at this level
            item.Affixes.Add(affix);
        }
        item.Name = MakeName(item, itemBase, rng);
        return item;
    }

    public static ItemBase PickBase(int itemLevel, Random rng)
    {
        var eligible = ItemCatalog.Bases.Where(b => b.DropLevel <= itemLevel).ToList();
        return Weighted(eligible, b => b.Weight, rng);
    }

    /// <summary>
    /// Magic find boosts magic items fully and rare items with diminishing returns (as in D2),
    /// so stacking it helps but never makes rares common.
    /// </summary>
    public static Rarity RollRarity(float magicFind, Random rng)
    {
        float mf = Math.Max(0f, magicFind);
        float magic = MagicWeight * (1f + mf / 100f);
        float rare = RareWeight * (1f + (mf * 250f / (mf + 250f)) / 100f);
        float roll = (float)rng.NextDouble() * (NormalWeight + magic + rare);
        if (roll < rare) return Rarity.Rare;
        if (roll < rare + magic) return Rarity.Magic;
        return Rarity.Normal;
    }

    static RolledAffix RollAffix(ItemInstance item, ItemSlot slot, int maxEach, Random rng)
    {
        var used = item.Affixes.Select(a => a.AffixId).ToHashSet();
        var openKinds = new List<AffixKind>();
        if (item.Prefixes.Count() < maxEach) openKinds.Add(AffixKind.Prefix);
        if (item.Suffixes.Count() < maxEach) openKinds.Add(AffixKind.Suffix);

        // Families that may roll here: right slot, right kind, unused, and with a tier this ilvl allows.
        var families = ItemCatalog.Affixes
            .Where(a => openKinds.Contains(a.Kind) && a.Slots.Contains(slot) && !used.Contains(a.Id))
            .Select(a => (def: a, tiers: a.Tiers.Where(t => t.MinItemLevel <= item.ItemLevel).ToList()))
            .Where(f => f.tiers.Count > 0)
            .ToList();
        if (families.Count == 0) return null;

        var (def, tiers) = Weighted(families, f => f.tiers.Sum(t => t.Weight), rng);
        var tier = Weighted(tiers, t => t.Weight, rng);
        return new RolledAffix
        {
            AffixId = def.Id, Kind = def.Kind, Tier = tier.Tier, Name = tier.Name,
            Stat = def.Stat, Value = RollValue(tier.Min, tier.Max, rng),
        };
    }

    static string MakeName(ItemInstance item, ItemBase itemBase, Random rng)
    {
        switch (item.Rarity)
        {
            case Rarity.Magic:
                string prefix = item.Prefixes.FirstOrDefault()?.Name;
                string suffix = item.Suffixes.FirstOrDefault()?.Name;
                return string.Join(" ", new[] { prefix, itemBase.Name, suffix }.Where(s => !string.IsNullOrEmpty(s)));
            case Rarity.Rare:
                var first = ItemCatalog.RareFirst[rng.Next(ItemCatalog.RareFirst.Length)];
                var seconds = ItemCatalog.RareSecond[itemBase.Slot];
                return $"{first} {seconds[rng.Next(seconds.Length)]}";
            default:
                return itemBase.Name;
        }
    }

    static float RollValue(float min, float max, Random rng) =>
        min >= max ? min : (float)Math.Round(min + rng.NextDouble() * (max - min));

    static T Weighted<T>(IReadOnlyList<T> items, Func<T, float> weight, Random rng)
    {
        float total = items.Sum(weight);
        float roll = (float)rng.NextDouble() * total;
        foreach (var it in items)
        {
            roll -= weight(it);
            if (roll < 0f) return it;
        }
        return items[items.Count - 1];
    }
}
