// Tooltip.cs – item details panel: name in rarity colour, base, item level, stat lines.
using Godot;

public partial class Tooltip : PanelContainer
{
    [Export] public Label NameLabel { get; set; }
    [Export] public Label BodyLabel { get; set; }

    public override void _Ready()
    {
        if (NameLabel == null || BodyLabel == null)
        {
            var box = new VBoxContainer();
            AddChild(box);
            NameLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
            BodyLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
            box.AddChild(NameLabel);
            box.AddChild(BodyLabel);
        }
        MouseFilter = MouseFilterEnum.Ignore;
        TopLevel = true; // position in screen space, above the grid
        Hide();
    }

    public void ShowItem(ItemInstance item, Vector2 at)
    {
        var lines = item.Describe().Split('\n', 2);
        NameLabel.Text = lines[0];
        NameLabel.Modulate = item.RarityColor;
        BodyLabel.Text = lines.Length > 1 ? lines[1] : "";
        GlobalPosition = at + new Vector2(16, 16);
        Show();
    }
}
