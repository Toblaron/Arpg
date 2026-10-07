// TestCombat.cs – fires one projectile with a dummy skill to check the effect wiring.
using Godot;

public partial class TestCombat : Node2D
{
    [Export] public ProjectilePool Pool { get; set; }

    public override void _Ready()
    {
        if (Pool == null) AddChild(Pool = new ProjectilePool());
        Pool.Get(new Vector2(100, 300), new Vector2(300, 0), damage: 10f, source: new DummySkill());
    }

    private sealed class DummySkill : ISkill
    {
        public string Id => "fireball";
        public SkillType Type => SkillType.Magic;
        public float Cooldown => 0f;
        public void Cast(Vector2 from, Vector2 target) { }
    }
}
