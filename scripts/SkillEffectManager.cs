// SkillEffectManager.cs – central place to trigger skill visuals.
using Godot;

public partial class SkillEffectManager : Control
{
    [Export] public SkillEffectAnimation Animation { get; set; }

    public void PlayEffect(ISkill skill) => Animation?.PlayAnimation(skill.Id);
}
