// VictoryAnimation.cs – plays the "victory" animation.
using Godot;

public partial class VictoryAnimation : Control
{
    [Export] public AnimationPlayer AnimationPlayer { get; set; }

    public void PlayAnimation()
    {
        if (AnimationPlayer != null && AnimationPlayer.HasAnimation("victory"))
            AnimationPlayer.Play("victory");
    }
}
