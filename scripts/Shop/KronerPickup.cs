// KronerPickup.cs – Norwegian kroner on the ground: coins for small amounts, a banknote for big
// ones. Pops out of the enemy, then slides to the player when they come close.
using Godot;

public partial class KronerPickup : Area2D
{
    private const float MagnetRange = 46f, MagnetSpeed = 260f, PickupRange = 10f;
    private static readonly Color Gold = new(1f, 0.8f, 0.18f), GoldDark = new(0.72f, 0.52f, 0.08f);
    private static readonly Color Note = new(0.55f, 0.42f, 0.25f), NoteDark = new(0.35f, 0.25f, 0.12f);

    public int Amount { get; private set; }
    private bool _collecting;
    private float _age;

    /// <summary>Drop <paramref name="amount"/> kroner near <paramref name="at"/>.</summary>
    public static void Drop(Node from, Vector2 at, int amount)
    {
        if (amount <= 0) return;
        var world = from.GetTree().CurrentScene ?? from.GetParent();
        var pickup = new KronerPickup { Amount = amount };
        Vector2 landing = PlayBounds.ClampDepth(at + new Vector2((float)GD.RandRange(-20, 20), (float)GD.RandRange(-6, 10)));
        pickup.Position = at;
        world.CallDeferred(Node.MethodName.AddChild, pickup); // may be called from a physics callback
        pickup.Ready += () =>
        {
            // Little pop: up and out, landing on the street.
            var t = pickup.CreateTween();
            t.TweenProperty(pickup, "position", landing + new Vector2(0, -14), 0.15f).SetEase(Tween.EaseType.Out);
            t.TweenProperty(pickup, "position", landing, 0.18f).SetEase(Tween.EaseType.In);
        };
    }

    public override void _Ready()
    {
        AddToGroup("Kroner");
        AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 8f } });
    }

    /// <summary>Fly to the player right away (stage cleared: sweep up what's left).</summary>
    public void CollectNow() => _collecting = true;

    public override void _PhysicsProcess(double delta)
    {
        _age += (float)delta;
        if (_age < 0.35f) return; // let the pop finish
        if (GetTree().GetFirstNodeInGroup("Player") is not Player player) return;
        Vector2 to = player.GlobalPosition - GlobalPosition;
        if (to.Length() < PickupRange)
        {
            player.AddKroner(Amount);
            FloatText.Spawn(player, $"+{Amount} kr", Gold);
            QueueFree();
            return;
        }
        if (_collecting || to.Length() < MagnetRange)
            GlobalPosition += to.Normalized() * Mathf.Min(to.Length(), MagnetSpeed * (_collecting ? 2.5f : 1f) * (float)delta);
        ZIndex = Mathf.FloorToInt(GlobalPosition.Y) - 1;
    }

    public override void _Draw()
    {
        DrawSetTransform(new Vector2(0, 5), 0f, new Vector2(1f, 0.35f));
        DrawCircle(Vector2.Zero, 6f, new Color(0, 0, 0, 0.3f));
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
        if (Amount >= 50)
        {
            // A banknote, folded over.
            DrawRect(new Rect2(-7, -4, 14, 8), Note);
            DrawRect(new Rect2(-7, -4, 14, 8), NoteDark, false, 0.8f);
            DrawCircle(new Vector2(2.5f, 0), 2.2f, NoteDark);
            DrawRect(new Rect2(-5.5f, -2.5f, 4, 1), NoteDark);
            DrawRect(new Rect2(-5.5f, 0.5f, 3, 1), NoteDark);
        }
        else
        {
            // A krone coin (with the hole in the middle, like the real 1 kr).
            DrawCircle(Vector2.Zero, 4.5f, GoldDark);
            DrawCircle(new Vector2(-0.4f, -0.4f), 3.8f, Gold);
            DrawCircle(new Vector2(-0.4f, -0.4f), 1f, GoldDark);
            if (Amount >= 15) // a little stack
            {
                DrawCircle(new Vector2(4, 2), 3.5f, GoldDark);
                DrawCircle(new Vector2(3.7f, 1.7f), 2.9f, Gold);
            }
        }
    }
}

/// <summary>Short text that pops up over a character and fades ("+25 kr").</summary>
public partial class FloatText : Label
{
    public static void Spawn(Node2D over, string text, Color color)
    {
        var world = over.GetTree().CurrentScene ?? over.GetParent();
        var label = new FloatText
        {
            Text = text,
            Position = over.GlobalPosition + new Vector2(-40, -50),
            Size = new Vector2(80, 12),
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
            ZIndex = (int)RenderingServer.CanvasItemZMax,
            LabelSettings = new LabelSettings { FontSize = 8, FontColor = color, OutlineSize = 3, OutlineColor = new Color(0, 0, 0, 0.7f) },
        };
        world.AddChild(label);
        var t = label.CreateTween();
        t.TweenProperty(label, "position:y", label.Position.Y - 16f, 0.7f);
        t.Parallel().TweenProperty(label, "modulate:a", 0f, 0.7f).SetDelay(0.3f);
        t.TweenCallback(Callable.From(label.QueueFree));
    }
}
