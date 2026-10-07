// Shockwave.cs – the boss's sledgehammer slam: a flat ring racing out along the street, with
// cracks and dust, plus a short camera shake.
using Godot;

public partial class Shockwave : Node2D
{
    private float _radius;
    private float _t;
    private const float Duration = 0.4f;

    public static void Spawn(Node from, Vector2 at, float radius)
    {
        var world = from.GetTree().CurrentScene ?? from.GetParent();
        world.AddChild(new Shockwave { _radius = radius, GlobalPosition = at, ZIndex = Mathf.FloorToInt(at.Y) - 2 });
        if (from.GetViewport().GetCamera2D() is { } cam)
        {
            var t = cam.CreateTween();
            for (int i = 0; i < 6; i++)
                t.TweenProperty(cam, "offset", new Vector2((float)GD.RandRange(-4, 4), (float)GD.RandRange(-3, 3)), 0.03f);
            t.TweenProperty(cam, "offset", Vector2.Zero, 0.04f);
        }
    }

    public override void _Process(double delta)
    {
        _t += (float)delta;
        if (_t >= Duration) QueueFree();
        QueueRedraw();
    }

    public override void _Draw()
    {
        float k = _t / Duration;
        float r = _radius * Mathf.Ease(k, 0.4f);
        DrawSetTransform(Vector2.Zero, 0f, new Vector2(1f, 0.32f)); // flat on the ground
        DrawArc(Vector2.Zero, r, 0f, Mathf.Tau, 48, new Color(1f, 0.9f, 0.7f, 1f - k), 5f);
        DrawArc(Vector2.Zero, r * 0.75f, 0f, Mathf.Tau, 40, new Color(0.8f, 0.7f, 0.6f, 0.6f * (1f - k)), 3f);
        DrawCircle(Vector2.Zero, r * 0.5f, new Color(0.6f, 0.55f, 0.5f, 0.25f * (1f - k)));
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
        var crack = new Color(0.1f, 0.08f, 0.08f, 1f - k);
        for (int i = 0; i < 6; i++)
        {
            Vector2 d = Vector2.FromAngle(i * Mathf.Tau / 6f + 0.3f) * new Vector2(1f, 0.32f);
            DrawLine(d * 6f, d * _radius * 0.45f, crack, 1.2f);
        }
    }
}
