// InventoryGrid.cs – the bag: a fixed number of slots holding items. Draws itself as a grid of
// InventorySlot buttons; hover shows the tooltip, click equips (if there's an EquipmentComponent).
using System;
using System.Collections.Generic;
using Godot;

public partial class InventoryGrid : GridContainer
{
    [Export] public int SlotCount { get; set; } = 40;
    [Export] public Tooltip Tooltip { get; set; }
    [Export] public EquipmentComponent Equipment { get; set; }
    /// <summary>Hidden until the player presses the "inventory" action (I), so it doesn't cover the street.</summary>
    [Export] public bool StartHidden { get; set; } = true;

    public event Action Changed;

    private ItemInstance[] _items;
    private readonly List<InventorySlot> _cells = new();

    public override void _Ready()
    {
        AddToGroup("Inventory");
        if (Columns <= 1) Columns = 10;
        _items ??= new ItemInstance[SlotCount];
        for (int i = 0; i < SlotCount; i++)
        {
            var cell = new InventorySlot { Index = i };
            cell.Hovered += OnHovered;
            cell.Activated += OnActivated;
            AddChild(cell);
            _cells.Add(cell);
        }
        Refresh();
        if (StartHidden) Hide();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed("inventory")) return;
        Visible = !Visible;
        if (!Visible) Tooltip?.Hide();
        GetViewport().SetInputAsHandled();
    }

    public ItemInstance GetItem(int index) => index >= 0 && index < SlotCount ? Items[index] : null;

    public void SetItem(int index, ItemInstance item)
    {
        if (index < 0 || index >= SlotCount) return;
        Items[index] = item;
        Refresh();
    }

    /// <summary>Put an item in the first free slot. False if the bag is full.</summary>
    public bool TryAdd(ItemInstance item)
    {
        int free = Array.IndexOf(Items, null);
        if (free < 0) return false;
        SetItem(free, item);
        return true;
    }

    public ItemInstance Take(int index)
    {
        var item = GetItem(index);
        if (item != null) SetItem(index, null);
        return item;
    }

    public void ClearAll()
    {
        Array.Clear(Items);
        Refresh();
    }

    private ItemInstance[] Items => _items ??= new ItemInstance[SlotCount];

    private void Refresh()
    {
        for (int i = 0; i < _cells.Count; i++) _cells[i].Display(Items[i]);
        Changed?.Invoke();
    }

    private void OnHovered(int index)
    {
        var item = GetItem(index);
        if (Tooltip == null) return;
        if (item == null) Tooltip.Hide(); else Tooltip.ShowItem(item, GetGlobalMousePosition());
    }

    private void OnActivated(int index)
    {
        var item = GetItem(index);
        if (item == null || Equipment == null) return;
        Take(index);
        var previous = Equipment.Equip(item);
        if (previous != null) SetItem(index, previous); // swap with what was worn
        Tooltip?.Hide();
    }
}
