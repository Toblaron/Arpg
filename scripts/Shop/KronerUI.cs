// KronerUI.cs – the kroner counter under the ability.
using Godot;

/// <summary>"1 250 kr" in gold, with a little bump whenever money comes in.</summary>
public partial class KronerUI : Label
{
    [Export] public Player Player { get; set; }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        LabelSettings = new LabelSettings { FontSize = 18, FontColor = new Color(1f, 0.8f, 0.18f), OutlineSize = 4, OutlineColor = new Color(0.19f, 0.04f, 0.28f) };
        PivotOffset = new Vector2(0, 12);
        Show(0);
        if (Player != null) Player.KronerChanged += Show;
    }

    private void Show(int kroner)
    {
        Text = ShopUI.Kr(kroner);
        Scale = new Vector2(1.2f, 1.2f);
        CreateTween().TweenProperty(this, "scale", Vector2.One, 0.15f);
    }
}
