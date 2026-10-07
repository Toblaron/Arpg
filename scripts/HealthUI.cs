// HealthUI.cs – low‑health warning visual cue
using Godot;

public partial class HealthUI : Control
{
    [Export] private ProgressBar _healthBar;
    [Export] private Color _lowHealthTint = new Color(1, 0.2f, 0.2f); // reddish
    [Export] private float _lowHealthThreshold = 0.3f; // 30%
    [Export] private float _pulseSpeed = 2f; // cycles per second

    private bool _isLowHealth = false;
    private float _pulseTimer = 0f;

    public void UpdateHealth(float current, float max)
    {
        float ratio = current / max;
        _healthBar.Value = ratio * 100f;

        bool nowLow = ratio <= _lowHealthThreshold;
        if (nowLow != _isLowHealth)
        {
            _isLowHealth = nowLow;
            if (!_isLowHealth)
                _healthBar.Modulate = Colors.White; // reset
        }
    }

    public override void _Process(double delta)
    {
        if (!_isLowHealth) return;

        _pulseTimer += (float)delta * _pulseSpeed;
        // sin goes -1..1, map to 0..1 then blend between white and tint
        float t = (Mathf.Sin(_pulseTimer * Mathf.Tau) + 1f) * 0.5f;
        _healthBar.Modulate = Colors.White.Lerp(_lowHealthTint, t);
    }

    // Stub retained for compatibility
    public void DisplayLowHealthWarning() { /* now handled in _Process */ }
}
