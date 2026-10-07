// ProjectilePool.cs – reuses projectile nodes instead of allocating one per shot.
using System.Collections.Generic;
using Godot;

public partial class ProjectilePool : Node2D
{
    /// <summary>Scene whose root is a <see cref="Projectile"/>; a bare Projectile is used if unset.</summary>
    [Export] public PackedScene ProjectileScene { get; set; }

    private readonly Queue<Projectile> _free = new();

    public Projectile Get(Vector2 position, Vector2 velocity, float damage, ISkill source = null)
    {
        var proj = _free.Count > 0 ? _free.Dequeue() : ProjectileScene?.Instantiate<Projectile>() ?? new Projectile();
        if (proj.GetParent() == null) AddChild(proj);
        proj.GlobalPosition = position;
        proj.Launch(velocity, damage, source);
        return proj;
    }

    public void Return(Projectile proj)
    {
        proj.Deactivate();
        _free.Enqueue(proj);
    }
}
