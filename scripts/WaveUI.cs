// WaveUI.cs – the beat-'em-up HUD messages: wave counter during a fight ("WAVE 3 · 4 left"),
// a big banner when a wave starts or the stage is cleared, and a blinking "GO →" when the
// screen unlocks and there's more street ahead.
using Godot;

public partial class WaveUI : Control
{
    [Export] public WaveSpawner Spawner { get; set; }
    [Export] public StageDirector Director { get; set; }

    private Label _counter;
    private Label _banner;
    private Label _go;
    private float _blink;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _counter = MakeLabel(16, new Vector2(0, 14), new Vector2(1152, 28));
        _banner = MakeLabel(40, new Vector2(0, 200), new Vector2(1152, 52));
        _banner.Modulate = new Color(1, 1, 1, 0);
        _go = MakeLabel(34, new Vector2(940, 250), new Vector2(190, 46));
        _go.Text = "GO →";
        _go.Visible = false;
        if (Spawner != null) Spawner.WaveStarted += (wave, _) => ShowBanner($"WAVE {wave}");
        if (Director != null)
        {
            Director.StageCleared += () => ShowBanner("STAGE CLEAR", 2f);
            Director.StageStarted += stage => ShowBanner($"STAGE {stage}");
            Director.BossAppeared += boss => ShowBanner(boss.Type.DisplayName.ToUpperInvariant());
        }
    }

    private Label MakeLabel(int size, Vector2 pos, Vector2 box)
    {
        var label = new Label
        {
            Position = pos,
            Size = box,
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
        string stage = $"STAGE {Director?.Stage ?? 1}";
        _counter.Text = Director?.BossFight == true ? $"{stage}  ·  BOSS"
            : Spawner is { InEncounter: true } s ? $"{stage}  ·  WAVE {s.CurrentWave}  ·  {s.Remaining} left"
            : stage;
        bool go = Director?.ShowGo ?? false;
        _blink = go ? _blink + (float)delta : 0f;
        _go.Visible = go && Mathf.PosMod(_blink, 0.8f) < 0.5f;
    }

    private void ShowBanner(string text, float hold = 1.2f)
    {
        _banner.Text = text;
        var t = CreateTween();
        t.TweenProperty(_banner, "modulate:a", 1f, 0.2f);
        t.TweenInterval(hold);
        t.TweenProperty(_banner, "modulate:a", 0f, 0.5f);
    }
}
