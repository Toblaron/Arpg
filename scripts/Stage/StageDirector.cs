// StageDirector.cs – runs a belt-scroller stage (Final Fight / Turtles in Time style):
// the camera follows the player to the right and never scrolls back; at each encounter point
// the screen locks until its waves are beaten, then "GO →" sends the player on. At the end of
// the street the boss is waiting; beating him clears the stage (and Matkroken opens). The next
// stage replays the street, harder: waves keep counting up and the boss gets tougher.
using Godot;

/// <summary>Where characters may stand. Set by the StageDirector; wide open when there is none.</summary>
public static class PlayBounds
{
    public static float Left = -1e6f, Right = 1e6f, Top = -1e6f, Bottom = 1e6f;

    public static Vector2 ClampPlayer(Vector2 p) => new(Mathf.Clamp(p.X, Left, Mathf.Max(Left, Right)), Mathf.Clamp(p.Y, Top, Mathf.Max(Top, Bottom)));

    /// <summary>The game's logical screen size (project setting; the window stretches to fit).</summary>
    public static Vector2 ScreenSize => new(
        (int)ProjectSettings.GetSetting("display/window/size/viewport_width"),
        (int)ProjectSettings.GetSetting("display/window/size/viewport_height"));
    public static Vector2 ClampDepth(Vector2 p) => new(p.X, Mathf.Clamp(p.Y, Top, Mathf.Max(Top, Bottom)));

    public static void Reset() => (Left, Right, Top, Bottom) = (-1e6f, 1e6f, -1e6f, 1e6f);
}

public partial class StageDirector : Node2D
{
    [Signal] public delegate void StageClearedEventHandler();
    [Signal] public delegate void StageStartedEventHandler(int stage);
    [Signal] public delegate void BossAppearedEventHandler(Enemy boss);

    [Export] public Player Player { get; set; }
    [Export] public Camera2D Camera { get; set; }
    [Export] public WaveSpawner Spawner { get; set; }

    [Export] public float StageLength { get; set; } = 5200f;
    /// <summary>Walkable strip (character centres): top of the sidewalk to the bottom of the road.</summary>
    [Export] public float FloorTop { get; set; } = 192f;
    [Export] public float FloorBottom { get; set; } = 302f;
    /// <summary>X positions where the screen locks for a fight, left to right.</summary>
    [Export] public float[] EncounterAt { get; set; } = { 1100f, 2200f, 3300f, 4450f };
    /// <summary>How many waves each encounter sends.</summary>
    [Export] public int[] EncounterWaves { get; set; } = { 1, 2, 2, 3 };
    /// <summary>X where the screen locks for the boss fight (after the last encounter).</summary>
    [Export] public float BossAt { get; set; } = 4650f;
    /// <summary>Where the player starts each stage.</summary>
    [Export] public Vector2 PlayerStart { get; set; } = new(90, 262);

    public bool Locked { get; private set; }
    public int NextEncounter { get; private set; }
    public bool Cleared { get; private set; }
    public int Stage { get; private set; } = 1;
    public Enemy Boss { get; private set; }
    public bool BossFight => Boss != null && IsInstanceValid(Boss);
    /// <summary>Show the blinking "GO →": free to move on and there's more street ahead.</summary>
    public bool ShowGo => !Locked && !Cleared && NextEncounter > 0;

    private bool _bossStarted;

    private const float Margin = 24f; // keep the player this far inside the screen edges
    private Vector2 _half;
    private float _camX;

    public override void _Ready()
    {
        // Half the visible area in world units (the camera is zoomed in).
        _half = PlayBounds.ScreenSize / 2f / (Camera?.Zoom ?? Vector2.One);
        PlayBounds.Top = FloorTop;
        PlayBounds.Bottom = FloorBottom;
        _camX = _half.X;
        if (Spawner != null) Spawner.EncounterCleared += OnEncounterCleared;
        UpdateCamera();
    }

    public override void _ExitTree() => PlayBounds.Reset();

    public override void _PhysicsProcess(double delta)
    {
        if (Player == null || !IsInstanceValid(Player)) return;

        if (!Locked)
        {
            // Follow forward only: the camera keeps the player at or left of centre.
            _camX = Mathf.Clamp(Mathf.Max(_camX, Player.GlobalPosition.X), _half.X, StageLength - _half.X);
            if (NextEncounter < EncounterAt.Length && Player.GlobalPosition.X >= EncounterAt[NextEncounter])
                StartEncounter();
            else if (!_bossStarted && NextEncounter >= EncounterAt.Length && Player.GlobalPosition.X >= BossAt)
                StartBoss();
        }
        UpdateCamera();
    }

    /// <summary>Jump the player and camera to <paramref name="x"/> (for testing and debugging).</summary>
    public void WarpTo(float x)
    {
        _camX = Mathf.Clamp(Mathf.Max(_camX, x), _half.X, StageLength - _half.X);
        UpdateCamera();
        Player.GlobalPosition = new Vector2(Mathf.Clamp(x, PlayBounds.Left, PlayBounds.Right), Player.GlobalPosition.Y);
    }

    private void StartEncounter()
    {
        Locked = true;
        int waves = NextEncounter < EncounterWaves.Length ? EncounterWaves[NextEncounter] : 1;
        NextEncounter++;
        Spawner?.StartEncounter(waves);
    }

    private void OnEncounterCleared()
    {
        if (!_bossStarted) Locked = false; // the boss's backup doesn't end the boss fight
    }

    private void StartBoss()
    {
        Locked = true;
        _bossStarted = true;
        Boss = Spawner?.SpawnBoss(Stage);
        if (Boss == null) { ClearStage(); return; }
        Boss.Killed += _ => ClearStage();
        EmitSignal(SignalName.BossAppeared, Boss);
    }

    private void ClearStage()
    {
        if (Cleared) return;
        Cleared = true;
        Locked = false;
        Boss = null;
        // Send the backup packing and sweep the leftover kroner into the player's pocket.
        foreach (var n in GetTree().GetNodesInGroup("Enemy"))
            if (n is Enemy { IsBoss: false } e) e.QueueFree();
        foreach (var n in GetTree().GetNodesInGroup("Kroner"))
            (n as KronerPickup)?.CollectNow();
        EmitSignal(SignalName.StageCleared);
    }

    /// <summary>Start stage <paramref name="stage"/> from the beginning of the street. Gear, kroner and upgrades carry over.</summary>
    public void StartStage(int stage)
    {
        Stage = stage;
        Cleared = Locked = _bossStarted = false;
        NextEncounter = 0;
        Boss = null;
        Spawner?.ClearAll();
        foreach (var n in GetTree().CurrentScene.GetChildren())
            if (n is Item || n is KronerPickup || n is FloatText) n.QueueFree();
        _camX = _half.X;
        UpdateCamera();
        Player.GlobalPosition = PlayerStart;
        EmitSignal(SignalName.StageStarted, Stage);
    }

    private void UpdateCamera()
    {
        if (Camera != null) Camera.GlobalPosition = new Vector2(_camX, _half.Y);
        var view = new Rect2(_camX - _half.X, 0, _half.X * 2f, _half.Y * 2f);
        PlayBounds.Left = view.Position.X + Margin;
        PlayBounds.Right = view.End.X - Margin;
        if (Spawner != null) Spawner.View = view;
    }
}
