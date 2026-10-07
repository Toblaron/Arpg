// SkillEffectManager.cs
using Godot;

public partial class SkillEffectManager : Control
{
    [Export] public SkillEffectAnimation Animation { get; set; }

    public void PlayEffect(ISkill skill)
    {
        Animation.PlayAnimation("fireball");
    }
}
