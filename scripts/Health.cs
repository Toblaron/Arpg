// Health.cs – hit points for any character (player or enemy). Add as a child node named "Health".
using Godot;

public partial class Health : Node
{
    [Signal] public delegate void HealthChangedEventHandler(float current, float max);
    [Signal] public delegate void DiedEventHandler();

    [Export] public float MaxHealth { get; set; } = 100f;
    public float Current { get; private set; }
    public bool IsDead => Current <= 0f;

    public override void _Ready()
    {
        Current = MaxHealth;
        EmitSignal(SignalName.HealthChanged, Current, MaxHealth);
    }

    public void ApplyDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;
        Current = Mathf.Max(Current - amount, 0f);
        EmitSignal(SignalName.HealthChanged, Current, MaxHealth);
        if (IsDead) EmitSignal(SignalName.Died);
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;
        Current = Mathf.Min(Current + amount, MaxHealth);
        EmitSignal(SignalName.HealthChanged, Current, MaxHealth);
    }

    /// <summary>Raise the maximum (e.g. from gear) keeping the same fraction of health.</summary>
    public void SetMaxHealth(float max)
    {
        float ratio = MaxHealth > 0 ? Current / MaxHealth : 1f;
        MaxHealth = Mathf.Max(1f, max);
        Current = MaxHealth * ratio;
        EmitSignal(SignalName.HealthChanged, Current, MaxHealth);
    }
}
