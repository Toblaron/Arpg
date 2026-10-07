// InventoryGrid.cs
public partial class InventoryGrid : Control
{
    // ...

    public string SerializeInventory()
    {
        // Serialize the inventory slots into a JSON string
        var dict = new Dictionary<string, string>();
        foreach (var slot in _slots.Values)
        {
            if (!slot.IsEmpty)
            {
                dict[slot.Name] = slot.Item.ResourcePath;
            }
        }
        return JsonSerializer.Serialize(dict);
    }
}

// EquipmentComponent.cs
public partial class EquipmentComponent : Node
{
    // ...

    public string Serialize()
    {
        // Serialize the equipment component into a JSON string
        var dict = new Dictionary<string, string>();
        foreach (var kvp in _slots)
        {
            dict[kvp.Key] = kvp.Value.IsEmpty ? null : kvp.Value.Item.ResourcePath;
        }
        return JsonSerializer.Serialize(dict);
    }
}
