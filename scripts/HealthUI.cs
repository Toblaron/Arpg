// HealthUI.cs – health bar with a pulsing low-health warning.
using Godot;

public partial class HealthUI : Control
{
    [Export] public Health Target { get; set; }
    [Export] public ProgressBar HealthBar { get; set; }
    [Export] public Color LowHealthTint { get; set; } = new Color(1, 0.2f, 0.2f);
    [Export] public float LowHealthThreshold { get; set; } = 0.3f; // 30%
    [Export] public float PulseSpeed { get; set; } = 2f; // cycles per second

    private bool _isLowHealth;
    private float _pulseTimer;

    public override void _Ready()
    {
        if (Target != null)
        {
            Target.HealthChanged += UpdateHealth;
            UpdateHealth(Target.Current, Target.MaxHealth);
        }
    }

    public void UpdateHealth(float current, float max)
    {
        if (HealthBar == null || max <= 0f) return;
        float ratio = current / max;
        HealthBar.Value = ratio * 100f;

        bool nowLow = ratio <= LowHealthThreshold;
        if (nowLow == _isLowHealth) return;
        _isLowHealth = nowLow;
        if (!_isLowHealth) HealthBar.Modulate = Colors.White;
    }

    public override void _Process(double delta)
    {
        if (!_isLowHealth || HealthBar == null) return;
        _pulseTimer += (float)delta * PulseSpeed;
        float t = (Mathf.Sin(_pulseTimer * Mathf.Tau) + 1f) * 0.5f; // 0..1
        HealthBar.Modulate = Colors.White.Lerp(LowHealthTint, t);
    }
}
