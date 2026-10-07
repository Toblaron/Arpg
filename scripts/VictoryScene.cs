// VictoryScene.cs
using Godot;

public partial class VictoryScene : Control
{
    [Export] public VictoryAnimation VictoryAnim { get; set; }

    public override void _Ready()
    {
        VictoryAnim.PlayAnimation();
    }
}
