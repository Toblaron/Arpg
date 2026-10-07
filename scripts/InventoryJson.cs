// InventoryJson.cs
using System.Text.Json;
using Godot;

public class InventoryJson : Control
{
    [Export] public string JsonData { get; set; } = "";

    public override void _Ready()
    {
        // Initialize the JSON data
        JsonData = SerializeInventory();
    }

    public void LoadInventory(Dictionary<string, string> data)
    {
        // Deserialize the JSON data
        var dict = JsonData.FromJson<Dictionary<string, string>>();
        // Update the inventory slots
        foreach (var kvp in dict)
        {
            if (string.IsNullOrEmpty(kvp.Value))
            {
                Unequip(kvp.Key);
            }
            else
            {
                var res = ResourceLoader.Load<ItemResource>(kvp.Value);
                if (res != null)
                    Equip(kvp.Key, res);
            }
        }
    }

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
