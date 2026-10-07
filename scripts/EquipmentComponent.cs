// EquipmentComponent.cs – what the character wears, and the stats it all adds up to.
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public enum EquipSlot { Weapon, Head, Chest, Gloves, Boots, Belt, Amulet, Ring1, Ring2 }

public partial class EquipmentComponent : Node
{
    /// <summary>Raised whenever something is equipped or removed.</summary>
    public event Action EquipmentChanged;

    private readonly Dictionary<EquipSlot, ItemInstance> _worn = new();
    private Stats _total = new();

    public IReadOnlyDictionary<EquipSlot, ItemInstance> Worn => _worn;
    public Stats TotalStats => _total;

    public float GetStat(StatKey key) => _total[key];

    public static bool Fits(ItemInstance item, EquipSlot slot) => item.Slot switch
    {
        ItemSlot.Ring => slot is EquipSlot.Ring1 or EquipSlot.Ring2,
        ItemSlot.Weapon => slot == EquipSlot.Weapon,
        ItemSlot.Head => slot == EquipSlot.Head,
        ItemSlot.Chest => slot == EquipSlot.Chest,
        ItemSlot.Gloves => slot == EquipSlot.Gloves,
        ItemSlot.Boots => slot == EquipSlot.Boots,
        ItemSlot.Belt => slot == EquipSlot.Belt,
        ItemSlot.Amulet => slot == EquipSlot.Amulet,
        _ => false,
    };

    /// <summary>Equip into the natural slot (an empty ring slot first). Returns what it replaced.</summary>
    public ItemInstance Equip(ItemInstance item)
    {
        var slots = Enum.GetValues<EquipSlot>().Where(s => Fits(item, s)).ToList();
        var target = slots.FirstOrDefault(s => !_worn.ContainsKey(s), slots.First());
        return Equip(target, item);
    }

    /// <summary>Equip into a specific slot. Returns the replaced item, or null.</summary>
    public ItemInstance Equip(EquipSlot slot, ItemInstance item)
    {
        if (item == null || !Fits(item, slot))
            throw new ArgumentException($"{item?.Name ?? "nothing"} can't go in {slot}");
        _worn.TryGetValue(slot, out var previous);
        _worn[slot] = item;
        Recalculate();
        return previous;
    }

    public ItemInstance Unequip(EquipSlot slot)
    {
        if (!_worn.Remove(slot, out var item)) return null;
        Recalculate();
        return item;
    }

    public void ClearAll()
    {
        _worn.Clear();
        Recalculate();
    }

    private void Recalculate()
    {
        _total = _worn.Values.Aggregate(new Stats(), (sum, item) => sum + item.GetTotalStats());
        EquipmentChanged?.Invoke();
    }
}
