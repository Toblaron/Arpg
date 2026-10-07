// Projectile.cs – add to existing class
using Game.Combat;

public partial class Projectile : Node2D
{
    [Export] public SkillEffectAnimation EffectAnim { get; set; }

    public void Init(ISkill skill)
    {
        // existing init logic …

        // play visual
        if (EffectAnim != null)
            EffectAnim.PlayAnimation(skill.Type.ToString().ToLower());
    }
}
