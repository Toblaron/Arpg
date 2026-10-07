// In your UI controller (e.g. InventoryGrid)
public partial class InventoryGrid : Control
{
    [Export] public EquipmentComponent Equipment { get; set; }
    [Export] public Tooltip Tooltip { get; set; }

    public override void _Ready()
    {
        Equipment.OnEquipmentChanged += UpdateTooltips;
        UpdateTooltips();
    }

    private void UpdateTooltips()
    {
        foreach (var slot in Equipment.Slots)
        {
            // Assuming each slot UI node has a name matching the equipment slot key
            var uiSlot = GetNode<Control>(slot.Key);
            uiSlot.Connect("mouse_entered", Callable.From(() => ShowTooltip(slot.Value)));
            uiSlot.Connect("mouse_exited", Callable.From(() => Tooltip.Hide()));
        }
    }

    private void ShowTooltip(InventorySlot slot)
    {
        if (slot.IsEmpty) return;
        Tooltip.UpdateTooltip(slot.Item);
        Tooltip.Show();
    }
}
