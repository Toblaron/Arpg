// Tooltip.cs
using Godot;

public partial class Tooltip : Control
{
    [Export] public Label ItemStatsLabel { get; set; }
    [Export] public Label AffixesLabel { get; set; }

    public void UpdateTooltip(ItemResource item)
    {
        ItemStatsLabel.Text = "Item Stats:\n" + item.GetTotalStats().ToString();
        AffixesLabel.Text = "Affixes:\n" + string.Join("\n", item.Affixes.Select(a => a.EffectDescription));
    }
}
