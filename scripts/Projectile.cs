// Projectile.cs – a pooled shot: flies, hits the first IDamageable it touches, returns to the pool.
using Godot;

public partial class Projectile : Area2D
{
    [Export] public float LifeTime { get; set; } = 5f;
    [Export] public SkillEffectAnimation EffectAnim { get; set; }

    public Vector2 Velocity { get; private set; }
    public float Damage { get; private set; }
    private float _elapsed;
    private bool _active;

    public override void _Ready()
    {
        if (GetNodeOrNull<CollisionShape2D>("CollisionShape2D") == null)
            AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 6f } });
        BodyEntered += OnHit;
    }

    public void Launch(Vector2 velocity, float damage, ISkill source)
    {
        Velocity = velocity;
        Damage = damage;
        _elapsed = 0f;
        _active = true;
        Visible = true;
        SetDeferred(Area2D.PropertyName.Monitoring, true);
        if (EffectAnim != null && source != null) EffectAnim.PlayAnimation(source.Id);
    }

    public void Deactivate()
    {
        _active = false;
        Visible = false;
        SetDeferred(Area2D.PropertyName.Monitoring, false);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_active) return;
        GlobalPosition += Velocity * (float)delta;
        _elapsed += (float)delta;
        if (_elapsed >= LifeTime) Finish();
    }

    private void OnHit(Node2D body)
    {
        if (!_active || body.IsInGroup("Player") || body is not IDamageable target) return;
        target.TakeDamage(Damage);
        Finish();
    }

    private void Finish()
    {
        if (GetParent() is ProjectilePool pool) pool.Return(this);
        else QueueFree();
    }
}
