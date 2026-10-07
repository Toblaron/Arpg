// SkillEffectAnimation.cs – thin wrapper around an AnimationPlayer so scenes can trigger named animations.
using Godot;

public partial class SkillEffectAnimation : Control
{
    [Export] public AnimationPlayer AnimationPlayer { get; set; }

    public void PlayAnimation(string animationName)
    {
        if (AnimationPlayer != null && AnimationPlayer.HasAnimation(animationName))
            AnimationPlayer.Play(animationName);
    }

    public void StopAnimation() => AnimationPlayer?.Stop();
}
