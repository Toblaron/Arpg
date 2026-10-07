// ProjectilePool.cs
using System.Collections.Generic;
using Godot;

namespace Game.Combat
{
    public partial class ProjectilePool : Node
    {
        private readonly Queue<Projectile> _pool = new();
        private readonly PackedScene _projectileScene = GD.Load<PackedScene>("res://scenes/Projectile.tscn");

        public Projectile Get(Vector2 position, Vector2 velocity)
        {
            Projectile proj = _pool.Count > 0 ? _pool.Dequeue() : (Projectile)_projectileScene.Instantiate();
            proj.GlobalPosition = position;
            proj.Velocity = velocity;
            proj.LifeTime = 5.0f; // default TTL
            AddChild(proj);
            proj.Visible = true;
            proj.Start();
            return proj;
        }

        public void Return(Projectile proj)
        {
            proj.Visible = false;
            RemoveChild(proj);
            _pool.Enqueue(proj);
        }
    }
}

// Projectile.cs
using Godot;

namespace Game.Combat
{
    public partial class Projectile : Area2D
    {
        public Vector2 Velocity { get; set; } = Vector2.Zero;
        public float LifeTime { get; set; } = 5.0f;
        private float _elapsed = 0f;

        public void Start()
        {
            _elapsed = 0f;
        }

        public override void _PhysicsProcess(double delta)
        {
            GlobalPosition += Velocity * (float)delta;
            _elapsed += (float)delta;
            if (_elapsed >= LifeTime)
            {
                // Return to pool
                GetParent<ProjectilePool>()?.Return(this);
            }
        }
    }
}

// TrajectoryHelper.cs
using Godot;

namespace Game.Combat
{
    public static class TrajectoryHelper
    {
        /// <summary>
        /// Returns the initial velocity vector to hit a target at `targetPos` from `startPos`
        /// given a fixed launch speed `speed` and gravity `gravity`.
        /// </summary>
        public static Vector2 CalculateLaunch(Vector2 startPos, Vector2 targetPos, float speed, float gravity)
        {
            Vector2 delta = targetPos - startPos;
            float distance = delta.Length();
            float angle = Mathf.Atan2(delta.Y, delta.X);
            // Simplified 2D projectile: ignore height differences for now
            float vx = speed * Mathf.Cos(angle);
            float vy = speed * Mathf.Sin(angle) - gravity * 0.5f;
            return new Vector2(vx, vy);
        }
    }
}
