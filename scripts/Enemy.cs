// Enemy.cs – chases the player, deals contact damage, drops loot on death.
//
// Expected children: CollisionShape2D, Health "Health", LootDrop "LootDrop" (optional).
using Godot;

public partial class Enemy : CharacterBody2D, IDamageable
{
    [Signal] public delegate void KilledEventHandler(Enemy enemy);

    [Export] public float Speed { get; set; } = 90f;
    [Export] public float ContactDamage { get; set; } = 8f;
    [Export] public float AttackRange { get; set; } = 28f;
    [Export] public float AttackCooldown { get; set; } = 1.0f;

    public Health Health { get; private set; }

    private Player _target;
    private float _cooldown;
    private float _lastY = float.MinValue;

    public override void _Ready()
    {
        AddToGroup("Enemy");
        Health = GetNodeOrNull<Health>("Health");
        if (Health != null) Health.Died += Die;
    }

    public override void _PhysicsProcess(double delta)
    {
        _cooldown = Mathf.Max(0f, _cooldown - (float)delta);
        _target ??= GetTree().GetFirstNodeInGroup("Player") as Player;
        if (_target == null || !IsInstanceValid(_target)) { Velocity = Vector2.Zero; return; }

        Vector2 toTarget = _target.GlobalPosition - GlobalPosition;
        if (toTarget.Length() > AttackRange)
        {
            Velocity = toTarget.Normalized() * Speed;
            MoveAndSlide();
        }
        else if (_cooldown <= 0f)
        {
            _target.TakeDamage(ContactDamage);
            _cooldown = AttackCooldown;
        }
        YSort.Update(this, ref _lastY);
    }

    public void TakeDamage(float amount) => Health?.ApplyDamage(amount);

    public void Die()
    {
        EmitSignal(SignalName.Killed, this);
        GetNodeOrNull<LootDrop>("LootDrop")?.Drop(GlobalPosition);
        QueueFree();
    }
}
