// WaveUI.cs – wave counter at the top of the screen ("WAVE 3 · 4 left"), a big banner when a
// wave starts, and a "cleared" message with the countdown to the next one.
using Godot;

public partial class WaveUI : Control
{
    [Export] public WaveSpawner Spawner { get; set; }

    private Label _counter;
    private Label _banner;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _counter = MakeLabel(16, new Vector2(0, 14));
        _banner = MakeLabel(40, new Vector2(0, 220));
        _banner.Modulate = new Color(1, 1, 1, 0);
        if (Spawner == null) return;
        Spawner.WaveStarted += OnWaveStarted;
        Spawner.WaveCleared += wave => ShowBanner($"WAVE {wave} CLEARED");
    }

    private Label MakeLabel(int size, Vector2 pos)
    {
        var label = new Label
        {
            Position = pos,
            Size = new Vector2(1152, size + 12),
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
            LabelSettings = new LabelSettings
            {
                FontSize = size,
                FontColor = PimpLook.Gold,
                OutlineSize = size / 4,
                OutlineColor = PimpLook.SuitDark,
            },
        };
        AddChild(label);
        return label;
    }

    public override void _Process(double delta)
    {
        if (Spawner == null || Spawner.CurrentWave == 0) { _counter.Text = ""; return; }
        _counter.Text = Spawner.BetweenWaves
            ? $"WAVE {Spawner.CurrentWave + 1} in {Mathf.CeilToInt(Spawner.TimeToNextWave)}"
            : $"WAVE {Spawner.CurrentWave}  ·  {Spawner.Remaining} left";
    }

    private void OnWaveStarted(int wave, int count) => ShowBanner($"WAVE {wave}");

    private void ShowBanner(string text)
    {
        _banner.Text = text;
        var t = CreateTween();
        t.TweenProperty(_banner, "modulate:a", 1f, 0.2f);
        t.TweenInterval(1.2f);
        t.TweenProperty(_banner, "modulate:a", 0f, 0.5f);
    }
}
