// SkillManager.cs – a character's skills: ticks cooldowns and casts by id.
using System.Collections.Generic;
using Godot;

public partial class SkillManager : Node
{
    private readonly Dictionary<string, ActiveSkill> _skills = new();

    public void Register(ActiveSkill skill) => _skills[skill.Id] = skill;

    public ActiveSkill Get(string id) => _skills.TryGetValue(id, out var s) ? s : null;

    public override void _Process(double delta)
    {
        foreach (var skill in _skills.Values) skill.Tick((float)delta);
    }

    public bool TryCast(string id, Vector2 from, Vector2 target)
    {
        if (!_skills.TryGetValue(id, out var skill) || !skill.CanCast()) return false;
        skill.Cast(from, target);
        return true;
    }
}
