// SkillEffect.cs
using Godot;

public partial class SkillEffect : Control
{
    [Export] public SkillEffectAnimation Animation { get; set; }

    public void PlayEffect(ISkill skill)
    {
        switch (skill.Type)
        {
            case SkillType.Fireball:
                Animation.PlayAnimation("fireball");
                break;
            case SkillType.Slash:
                Animation.PlayAnimation("slash");
                break;
        }
    }
}
