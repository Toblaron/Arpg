// Skill.cs – what every skill exposes. Id doubles as the effect animation name ("fireball", "slash").
using Godot;

public enum SkillType { Melee, Ranged, Magic }

public interface ISkill
{
    string Id { get; }
    SkillType Type { get; }
    float Cooldown { get; }
    void Cast(Vector2 from, Vector2 target);
}
