// AbilityUI.cs – shows the player's class ability and its cooldown, e.g. "Bitch-Slap [K]  READY".
using Godot;

public partial class AbilityUI : Label
{
    [Export] public Player Player { get; set; }
    [Export] public string KeyHint { get; set; } = "K";

    public override void _Process(double delta)
    {
        if (Player?.Class == null) { Text = ""; return; }
        float left = Player.AbilityCooldownRemaining;
        Text = left > 0f
            ? $"{Player.Class.AbilityName} [{KeyHint}]  {left:0.0}s"
            : $"{Player.Class.AbilityName} [{KeyHint}]  READY";
        Modulate = left > 0f ? new Color(0.7f, 0.7f, 0.7f) : PimpLook.Gold;
    }
}
