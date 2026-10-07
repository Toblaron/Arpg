// InventorySlot.cs – one cell of the inventory grid: shows the item's base name in rarity colour.
using System;
using Godot;

public partial class InventorySlot : Button
{
    public int Index { get; set; }
    public event Action<int> Hovered;
    public event Action<int> Activated;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(64, 64);
        ClipText = true;
        AddThemeFontSizeOverride("font_size", 10);
        MouseEntered += () => Hovered?.Invoke(Index);
        MouseExited += () => Hovered?.Invoke(-1);
        Pressed += () => Activated?.Invoke(Index);
    }

    public void Display(ItemInstance item)
    {
        Text = item?.BaseName ?? "";
        Modulate = item?.RarityColor ?? Colors.White;
        TooltipText = ""; // the custom Tooltip panel shows details
    }
}
