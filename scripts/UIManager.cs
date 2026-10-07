// UIManager.cs
using Godot;

public partial class UIManager : Control
{
    [Export] public AnimationPlayer AnimationPlayer { get; set; }

    public void PlayAnimation(string animationName)
    {
        AnimationPlayer.Play(animationName);
    }

    public void StopAnimation()
    {
        AnimationPlayer.Stop();
    }
}
