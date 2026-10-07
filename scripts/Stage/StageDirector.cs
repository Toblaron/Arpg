// StageDirector.cs – runs a belt-scroller stage (Final Fight / Turtles in Time style):
// the camera follows the player to the right and never scrolls back; at each encounter point
// the screen locks until its waves are beaten, then "GO →" sends the player on. Reaching the
// end of the street clears the stage.
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

    public bool Locked { get; private set; }
    public int NextEncounter { get; private set; }
    public bool Cleared { get; private set; }
    /// <summary>Show the blinking "GO →": free to move on and there's more street ahead.</summary>
    public bool ShowGo => !Locked && !Cleared && NextEncounter > 0;

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
            else if (!Cleared && NextEncounter >= EncounterAt.Length && Player.GlobalPosition.X >= StageLength - 160f)
                ClearStage();
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

    private void OnEncounterCleared() => Locked = false;

    private void ClearStage()
    {
        Cleared = true;
        EmitSignal(SignalName.StageCleared);
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
