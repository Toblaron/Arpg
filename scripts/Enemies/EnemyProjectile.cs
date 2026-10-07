// EnemyProjectile.cs – a bottle thrown by an enemy: spins through the air, hurts the player on
// contact, smashes after a few hundred pixels.
using Godot;

public partial class EnemyProjectile : Area2D
{
    private const float Speed = 260f, Range = 320f;

    private Vector2 _velocity;
    private float _damage;
    private float _travelled;

    public static void Throw(Node thrower, Vector2 from, Vector2 dir, float damage)
    {
        var world = thrower.GetTree().CurrentScene ?? thrower.GetParent();
        var bottle = new EnemyProjectile
        {
            _velocity = dir * Speed,
            _damage = damage,
            GlobalPosition = from,
            ZIndex = (int)RenderingServer.CanvasItemZMax - 2,
        };
        world.AddChild(bottle);
    }

    public override void _Ready()
    {
        AddToGroup("EnemyProjectile");
        AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 6f } });
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 step = _velocity * (float)delta;
        GlobalPosition += step;
        Rotation += 14f * (float)delta;
        _travelled += step.Length();
        if (_travelled > Range) QueueFree();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Player player) return;
        player.TakeDamage(_damage);
        QueueFree();
    }

    public override void _Draw()
    {
        // A green glass bottle.
        DrawRect(new Rect2(-2.5f, -3f, 5f, 8f), new Color(0.2f, 0.55f, 0.25f));
        DrawRect(new Rect2(-1.2f, -7f, 2.4f, 4.5f), new Color(0.2f, 0.55f, 0.25f));
        DrawRect(new Rect2(-1.6f, -1f, 3.2f, 3f), new Color(0.95f, 0.9f, 0.75f)); // label
        DrawLine(new Vector2(-1.5f, -2f), new Vector2(-1.5f, 4f), new Color(1, 1, 1, 0.5f), 0.8f);
    }
}
