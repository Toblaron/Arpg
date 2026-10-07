// AffixGenerator.cs – corrected slot selection and percent handling
using Godot;
using System.Collections.Generic;
using System.Linq;

public static class AffixGenerator
{
    private static readonly Dictionary<ItemResource.Rarity, (int min, int max)> RaritySlotTable = new()
    {
        { ItemResource.Rarity.Common, (0, 1) },
        { ItemResource.Rarity.Magic,  (1, 2) },
        { ItemResource.Rarity.Rare,   (2, 3) },
        { ItemResource.Rarity.Unique, (3, 5) },
    };

    public static ItemResource Roll(ItemResource template, List<AffixDef> affixPool)
    {
        // Clone the template so the original asset stays pristine
        var item = (ItemResource)template.Duplicate();
        item.Affixes = new List<AffixDef>();

        var (minSlots, maxSlots) = RaritySlotTable[item.ItemRarity];
        int slotCount = GD.RandRange(minSlots, maxSlots + 1); // max inclusive as intended

        // Weighted sampling without replacement
        var pool = new List<AffixDef>(affixPool);
        for (int i = 0; i < slotCount && pool.Count > 0; i++)
        {
            float totalWeight = pool.Sum(a => a.Weight);
            float roll = GD.Randf() * totalWeight;
            float accum = 0f;
            AffixDef chosen = null;
            foreach (var affix in pool)
            {
                accum += affix.Weight;
                if (roll <= accum)
                {
                    chosen = affix;
                    break;
                }
            }
            if (chosen != null)
            {
                // Clone the affix to avoid mutating the original resource
                var affixInst
