// InventorySerializer.cs – saves the bag and worn gear to JSON, rolls included.
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

public static class InventorySerializer
{
    public const string DefaultPath = "user://savegame.json";

    private sealed class SavedSlot
    {
        public int Index { get; set; }
        public ItemInstance Item { get; set; }
    }

    private sealed class SaveState
    {
        public int Version { get; set; } = 1;
        public List<SavedSlot> Inventory { get; set; } = new();
        public Dictionary<EquipSlot, ItemInstance> Equipped { get; set; } = new();
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }, // "Rare", "FireResistance": readable and stable
    };

    public static string Save(InventoryGrid grid, EquipmentComponent equipment)
    {
        var state = new SaveState();
        for (int i = 0; i < grid.SlotCount; i++)
        {
            var item = grid.GetItem(i);
            if (item != null) state.Inventory.Add(new SavedSlot { Index = i, Item = item });
        }
        foreach (var (slot, item) in equipment.Worn) state.Equipped[slot] = item;
        return JsonSerializer.Serialize(state, Options);
    }

    public static void Load(InventoryGrid grid, EquipmentComponent equipment, string json)
    {
        var state = JsonSerializer.Deserialize<SaveState>(json, Options);
        if (state == null) return;
        grid.ClearAll();
        equipment.ClearAll();
        foreach (var saved in state.Inventory)
            if (saved.Item?.Base != null) grid.SetItem(saved.Index, saved.Item); // skip bases removed since
        foreach (var (slot, item) in state.Equipped)
            if (item?.Base != null && EquipmentComponent.Fits(item, slot)) equipment.Equip(slot, item);
    }

    public static void SaveToFile(InventoryGrid grid, EquipmentComponent equipment, string path = DefaultPath)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file?.StoreString(Save(grid, equipment));
    }

    public static bool LoadFromFile(InventoryGrid grid, EquipmentComponent equipment, string path = DefaultPath)
    {
        if (!FileAccess.FileExists(path)) return false;
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) return false;
        Load(grid, equipment, file.GetAsText());
        return true;
    }
}
