// ActiveSkill.cs – skills with cooldowns. Damage scales from the caster's gear.
using System;
using Godot;

public abstract class ActiveSkill : ISkill
{
    public string Id { get; }
    public SkillType Type { get; }
    public float Cooldown { get; }
    public float CooldownRemaining => Math.Max(0f, Cooldown - _sinceLastCast);
    private float _sinceLastCast;

    protected ActiveSkill(string id, SkillType type, float cooldown)
    {
        Id = id;
        Type = type;
        Cooldown = cooldown;
        _sinceLastCast = cooldown; // ready immediately
    }

    public bool CanCast() => _sinceLastCast >= Cooldown;

    public void Tick(float delta) => _sinceLastCast += delta;

    public void Cast(Vector2 from, Vector2 target)
    {
        if (!CanCast()) return;
        _sinceLastCast = 0f;
        Execute(from, target);
    }

    protected abstract void Execute(Vector2 from, Vector2 target);
}

/// <summary>Fires a pooled projectile toward the target.</summary>
public sealed class FireballSkill : ActiveSkill
{
    private readonly ProjectilePool _pool;
    private readonly Func<float> _damage;

    public FireballSkill(ProjectilePool pool, Func<float> damage) : base("fireball", SkillType.Magic, 2.5f)
    {
        _pool = pool;
        _damage = damage;
    }

    protected override void Execute(Vector2 from, Vector2 target)
    {
        var dir = (target - from).Normalized();
        _pool.Get(from, dir * 400f, _damage(), this);
    }
}

/// <summary>Hits every enemy in a small arc in front of the caster.</summary>
public sealed class SlashSkill : ActiveSkill
{
    private readonly SpatialGrid _grid;
    private readonly Func<float> _damage;
    private readonly float _reach;

    public SlashSkill(SpatialGrid grid, Func<float> damage, float reach = 40f) : base("slash", SkillType.Melee, 0.5f)
    {
        _grid = grid;
        _damage = damage;
        _reach = reach;
    }

    protected override void Execute(Vector2 from, Vector2 target)
    {
        var center = from + (target - from).LimitLength(_reach);
        foreach (var node in _grid.Query(center, _reach))
            (node as IDamageable)?.TakeDamage(_damage());
    }
}
