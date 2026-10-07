// CaneSweepEffect.cs – the trail of the Pimp's Cane Sweep: a violet arc with a gold edge along the
// path of the cane's tip, from low behind him, under and up through the front. Sparks when it connects.
using Godot;

public partial class CaneSweepEffect : Node2D
{
    private const float Duration = 0.22f;
    private const float Radius = 29f; // shoulder to cane tip (PimpLook's CaneArm)
    private bool _landed;
    private float _t;

    public static void Spawn(Player player, bool landed)
    {
        var world = player.GetTree().CurrentScene ?? player.GetParent();
        world.AddChild(new CaneSweepEffect
        {
            _landed = landed,
            Position = player.GlobalPosition + new Vector2(player.Facing.X * 6f, -9f), // the shoulder pivot
            Scale = new Vector2(player.Facing.X < 0 ? -1f : 1f, 1f),
            ZIndex = (int)RenderingServer.CanvasItemZMax - 1,
        });
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
        float fade = 1f - k;
        // The tip runs from low behind (160°) down under him (90°) and up in front (-23°), matching the
        // arm's wind-up (+1.3 rad) and follow-through (-1.9 rad); the trail chases it.
        float start = Mathf.DegToRad(160f), end = Mathf.DegToRad(-23f);
        float head = Mathf.Lerp(start, end, Mathf.Ease(Mathf.Min(1f, k * 1.6f), 0.5f));
        float tail = Mathf.Min(start, head + 1.6f);
        DrawArc(Vector2.Zero, Radius, tail, head, 20, new Color(PimpLook.Feather, 0.35f * fade), 9f);
        DrawArc(Vector2.Zero, Radius + 3f, tail, head, 20, new Color(PimpLook.Gold, 0.85f * fade), 2f);
        DrawArc(Vector2.Zero, Radius - 4f, tail, head, 20, new Color(1, 1, 1, 0.4f * fade), 1.5f);
        DrawCircle(Vector2.FromAngle(head) * (Radius + 2f), 3f * fade + 1f, new Color(PimpLook.Gold, fade));
        if (!_landed) return;
        Vector2 hit = Vector2.FromAngle(Mathf.DegToRad(15f)) * (Radius + 4f);
        for (int i = 0; i < 6; i++)
        {
            Vector2 d = Vector2.FromAngle(i * Mathf.Tau / 6f + 0.3f);
            DrawLine(hit + d * (2f + 6f * k), hit + d * (6f + 10f * k), new Color(PimpLook.Gold, fade), 1.5f);
        }
    }
}
