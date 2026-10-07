// LoadAnimation.cs – plays the "loading" animation.
using Godot;

public partial class LoadAnimation : Control
{
    [Export] public AnimationPlayer AnimationPlayer { get; set; }

    public void PlayAnimation()
    {
        if (AnimationPlayer != null && AnimationPlayer.HasAnimation("loading"))
            AnimationPlayer.Play("loading");
    }
}
