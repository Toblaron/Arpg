// ItemResource.cs – updated model with total‑stats helper
using Godot;
using System.Collections.Generic;

[Tool]
public partial class ItemResource : Resource
{
    public enum Rarity { Common, Magic, Rare, Unique }

    [Export] public string ItemName { get; set; } = "Unnamed";
    [Export] public Rarity ItemRarity { get; set; } = Rarity.Common;

    // Base stats for a clean template (e.g. {Damage:10, Health:100})
    [Export] public Dictionary<StatKey, float> BaseStats { get; set; } = new();

    // Runtime list of affixes attached to this instance
    public List<AffixDef> Affixes { get; set; } = new();

    // Calculates final stats = base + affix contributions
    public Dictionary<StatKey, float> GetTotalStats()
    {
        var total = new Dictionary<StatKey, float>(BaseStats);
        foreach (var affix in Affixes)
        {
            if (!total.ContainsKey(affix.TargetStat))
                total[affix.TargetStat] = 0f;

            float add = affix.EffectType == AffixEffectType.Percent
                ? total[affix.TargetStat] * affix.Magnitude   // magnitude already expressed as 0.1 for 10%
                : affix.Magnitude;

            total[affix.TargetStat] += add;
        }
        return total;
    }
}
