// BossUI.cs – the boss's name and health bar at the bottom of the screen.
using Godot;

/// <summary>Boss name and a long health bar, shown only while a boss is alive.</summary>
public partial class BossUI : Control
{
    private Label _name;
    private ProgressBar _bar;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _name = new Label
        {
            Position = new Vector2(276, 556), Size = new Vector2(600, 24), HorizontalAlignment = HorizontalAlignment.Center,
            LabelSettings = new LabelSettings { FontSize = 18, FontColor = Colors.White, OutlineSize = 5, OutlineColor = new Color(0.3f, 0.02f, 0.04f) },
        };
        _bar = new ProgressBar { Position = new Vector2(276, 582), Size = new Vector2(600, 20), ShowPercentage = false, MaxValue = 1 };
        _bar.AddThemeStyleboxOverride("fill", new StyleBoxFlat { BgColor = new Color(0.8f, 0.1f, 0.14f) });
        _bar.AddThemeStyleboxOverride("background", new StyleBoxFlat { BgColor = new Color(0.1f, 0.05f, 0.06f, 0.85f), BorderColor = new Color(1f, 0.8f, 0.18f), BorderWidthTop = 2, BorderWidthBottom = 2, BorderWidthLeft = 2, BorderWidthRight = 2 });
        AddChild(_name);
        AddChild(_bar);
        Hide();
    }

    public override void _Process(double delta)
    {
        var boss = GetTree().GetFirstNodeInGroup("Boss") as Enemy;
        bool show = boss != null && IsInstanceValid(boss) && !boss.IsQueuedForDeletion() && boss.Health != null;
        Visible = show;
        if (!show) return;
        _name.Text = boss.Type.DisplayName.ToUpperInvariant();
        _bar.Value = boss.Health.Current / boss.Health.MaxHealth;
    }
}
