// InventorySlot.cs
using Godot;

public partial class InventorySlot : Control
{
    [Export] public int Capacity { get; set; } = 1;
    [Export] public int StackCount { get; set; } = 1;
    [Export] public ItemResource Item { get; set; } = null;

    public void AssignItem(ItemResource item)
    {
        Item = item;
        // Update the slot's visuals and stats
        // ...
    }
}
