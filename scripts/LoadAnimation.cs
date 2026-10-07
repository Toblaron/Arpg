// LoadAnimation.cs
using Godot;

public partial class LoadAnimation : Control
{
    [Export] public AnimationPlayer AnimationPlayer { get; set; }

    public void PlayAnimation()
    {
        AnimationPlayer.PlayAnimation("loading");
    }
}
