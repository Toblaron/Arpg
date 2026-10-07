// SkillEffect.cs – plays the visual for a skill (animation named after the skill id).
using Godot;

public partial class SkillEffect : Control
{
    [Export] public SkillEffectAnimation Animation { get; set; }

    public void PlayEffect(ISkill skill) => Animation?.PlayAnimation(skill.Id);
}
