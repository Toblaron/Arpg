using System;
using System.Linq;
using System.Collections.Generic;

var rng = new Random(42);
int fails = 0;
void Check(bool ok, string what) { if (!ok) { fails++; if (fails < 10) Console.WriteLine("FAIL: " + what); } }

foreach (var (ilvl, mf) in new[] { (1, 0f), (12, 0f), (35, 0f), (35, 300f) })
{
    var counts = new Dictionary<Rarity, int>();
    for (int i = 0; i < 5000; i++)
    {
        var it = ItemGenerator.Generate(ilvl, mf, rng);
        counts[it.Rarity] = counts.GetValueOrDefault(it.Rarity) + 1;
        var b = it.Base;
        Check(b != null && b.DropLevel <= ilvl, $"base {it.BaseId} dropLevel>{ilvl}");
        int p = it.Prefixes.Count(), s = it.Suffixes.Count();
        if (it.Rarity == Rarity.Normal) Check(p + s == 0, "normal has affixes");
        if (it.Rarity == Rarity.Magic) Check(p <= 1 && s <= 1 && p + s >= 1, $"magic p{p} s{s}");
        if (it.Rarity == Rarity.Rare) Check(p <= 3 && s <= 3 && p + s >= 3, $"rare p{p} s{s} {it.BaseId}");
        Check(it.Affixes.Select(a => a.AffixId).Distinct().Count() == it.Affixes.Count, "duplicate family");
        foreach (var a in it.Affixes)
        {
            var def = ItemCatalog.Affixes.First(d => d.Id == a.AffixId);
            var tier = def.Tiers.First(t => t.Tier == a.Tier);
            Check(def.Slots.Contains(it.Slot), $"{a.AffixId} on {it.Slot}");
            Check(tier.MinItemLevel <= ilvl, $"tier {a.Tier} of {a.AffixId} at ilvl {ilvl}");
            Check(a.Value >= tier.Min && a.Value <= tier.Max, $"value {a.Value} outside {tier.Min}-{tier.Max}");
        }
        Check(!string.IsNullOrWhiteSpace(it.Name), "empty name");
    }
    Console.WriteLine($"ilvl {ilvl,2} mf {mf,3}: " + string.Join("  ", counts.OrderBy(k => k.Key).Select(k => $"{k.Key} {k.Value * 100.0 / 5000:0.0}%")));
}
Console.WriteLine(fails == 0 ? "ALL RULES HOLD" : $"{fails} failures");
if (fails > 0) Environment.Exit(1);
Console.WriteLine();
var shown = new HashSet<Rarity>();
var r2 = new Random(7);
while (shown.Count < 3 || shown.Count(x => x == Rarity.Rare) < 1)
{
    var it = ItemGenerator.Generate(30, 100, r2);
    if (shown.Add(it.Rarity)) { Console.WriteLine($"[{it.Rarity}]\n{it.Describe()}\n"); }
}
for (int k = 0; k < 2; k++) { ItemInstance it; do it = ItemGenerator.Generate(30, 150, r2); while (it.Rarity != Rarity.Rare); Console.WriteLine($"[Rare]\n{it.Describe()}\n"); }
