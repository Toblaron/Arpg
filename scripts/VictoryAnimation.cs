// VictoryAnimation.cs
using Godot;

public partial class VictoryAnimation : Control
{
    [Export] public AnimationPlayer AnimationPlayer { get; set; }

    public void PlayAnimation()
    {
        AnimationPlayer.PlayAnimation("victory");
    }
}
