// ActiveSkill.cs
using Godot;
using System;

namespace Game.Combat
{
    /// <summary>
    /// Base class for any skill that has a cooldown and can be cast.
    /// </summary>
    public abstract class ActiveSkill : ISkill
    {
        public SkillType Type { get; protected set; }
        public float Cooldown { get; protected set; }
        private float _timeSinceLastCast;

        public ActiveSkill(SkillType type, float cooldown)
        {
            Type = type;
            Cooldown = cooldown;
            _timeSinceLastCast = cooldown; // ready to use
        }

        /// <summary>
        /// Returns true if the skill can be cast right now.
        /// </summary>
        public bool CanCast() => _timeSinceLastCast >= Cooldown;

        /// <summary>
        /// Call this every frame to count cooldown.
        /// </summary>
        public void Tick(float delta) => _timeSinceLastCast += delta;

        /// <summary>
        /// Perform the actual casting logic.
        /// </summary>
        public void Cast(Vector2 from, Vector2 target)
        {
            if (!CanCast()) return;
            _timeSinceLastCast = 0f;
            Execute(from, target);
        }

        protected abstract void Execute(Vector2 from, Vector2 target);
    }

    // Example: a simple fireball skill
    public class FireballSkill : ActiveSkill
    {
        private readonly ProjectilePool _pool;
        private readonly float _damage;

        public FireballSkill(ProjectilePool pool, float damage)
            : base(SkillType.Ranged, 2.5f) // 2.5s cooldown
        {
            _pool = pool;
            _damage = damage;
        }

        protected override void Execute(Vector2 from, Vector2 target)
        {
            var dir = (target - from).Normalized();
            var proj = _pool.Get(from, dir * 400f); // 400 units/s speed
            proj.SetDamage(_damage);
        }
    }

    // SkillManager – keeps track of all active skills per player
    public partial class SkillManager : Node
    {
        private readonly Dictionary<string, ActiveSkill> _skills = new();
        private readonly ProjectilePool _pool;

        public SkillManager(ProjectilePool pool) => _pool = pool;

        public void Register(string id, ActiveSkill skill) => _skills[id] = skill;

        public void Update(float delta)
        {
            foreach (var skill in _skills.Values)
                skill.Tick(delta);
        }

        public void TryCast(string id, Vector2 from, Vector2 target)
        {
            if (_skills.TryGetValue(id, out var skill))
                skill.Cast(from, target);
        }
    }
}
