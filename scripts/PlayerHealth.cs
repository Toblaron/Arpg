// PlayerHealth.cs
using Godot;

public partial class PlayerHealth : Node
{
    [Export] public int MaxHealth = 100;
    public int CurrentHealth { get; private set; }

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
    }

    public void ApplyDamage(int amount)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        GD.Print($"Player HP: {CurrentHealth}");
        if (CurrentHealth <= 0)
            GetTree().Quit(); // placeholder death handling
    }
}
