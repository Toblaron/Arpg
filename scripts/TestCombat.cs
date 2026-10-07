// TestCombat.cs
using Godot;
using Game.Combat;

public partial class TestCombat : Node2D
{
    [Export] public PackedScene ProjectileScene { get; set; }

    public override void _Ready()
    {
        var proj = (Projectile)ProjectileScene.Instantiate();
        AddChild(proj);

        // Mock skill
        var skill = new DummySkill(SkillType.Fireball);
        proj.Init(skill);
    }

    private class DummySkill : ISkill
    {
        public SkillType Type { get; }
        public DummySkill(SkillType type) => Type = type;
    }
}
