// SlapEffect.cs – the Bitch-Slap impact: white swoosh arcs, impact stars and a "SLAP!" pop-up.
using Godot;

public partial class SlapEffect : Node2D
{
    private const float Duration = 0.35f;
    private bool _landed;

    public static void Spawn(Player player, bool landed)
    {
        var world = player.GetTree().CurrentScene ?? player.GetParent();
        var fx = new SlapEffect
        {
            _landed = landed,
            Position = player.GlobalPosition + player.Facing * 34f + new Vector2(0, -6),
            Scale = new Vector2(player.Facing.X < 0 ? -0.6f : 0.6f, 0.6f),
            ZIndex = (int)RenderingServer.CanvasItemZMax - 1, // above the Y-sorted characters
        };
        world.AddChild(fx);

        var text = new Label
        {
            Text = landed ? "SLAP!" : "swish",
            LabelSettings = new LabelSettings
            {
                FontSize = landed ? 12 : 8,
                FontColor = landed ? PimpLook.Gold : new Color(1, 1, 1, 0.8f),
                OutlineSize = landed ? 4 : 2,
                OutlineColor = PimpLook.SuitDark,
            },
            Position = player.GlobalPosition + new Vector2(-30, -56),
            Size = new Vector2(60, 18),
            HorizontalAlignment = HorizontalAlignment.Center,
            ZIndex = (int)RenderingServer.CanvasItemZMax,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            PivotOffset = new Vector2(30, 9),
        };
        world.AddChild(text);
        var t = text.CreateTween();
        t.TweenProperty(text, "scale", new Vector2(1.3f, 1.3f), 0.08f);
        t.TweenProperty(text, "scale", Vector2.One, 0.1f);
        t.Parallel().TweenProperty(text, "position:y", text.Position.Y - 14f, 0.6f);
        t.TweenProperty(text, "modulate:a", 0f, 0.25f);
        t.TweenCallback(Callable.From(text.QueueFree));
    }

    public override void _Ready()
    {
        float sx = Mathf.Sign(Scale.X);
        var t = CreateTween();
        t.TweenProperty(this, "scale", new Vector2(1.25f * sx, 1.25f), Duration).SetEase(Tween.EaseType.Out);
        t.Parallel().TweenProperty(this, "modulate:a", 0f, Duration);
        t.TweenCallback(Callable.From(QueueFree));
    }

    public override void _Draw()
    {
        var white = new Color(1, 1, 1, 0.9f);
        DrawArc(new Vector2(-14, 0), 24f, -1.1f, 1.1f, 18, white, 3f);
        DrawArc(new Vector2(-14, 0), 18f, -0.9f, 0.9f, 14, new Color(1, 1, 1, 0.5f), 2f);
        if (!_landed) return;
        // Impact star.
        for (int i = 0; i < 8; i++)
        {
            Vector2 d = Vector2.FromAngle(i * Mathf.Tau / 8f);
            DrawLine(new Vector2(12, 0) + d * 5f, new Vector2(12, 0) + d * (i % 2 == 0 ? 15f : 10f), PimpLook.Gold, 2f);
        }
    }
}
