// VictoryScene.cs – plays the victory animation when the scene opens.
using Godot;

public partial class VictoryScene : Control
{
    [Export] public VictoryAnimation VictoryAnim { get; set; }

    public override void _Ready() => VictoryAnim?.PlayAnimation();
}
