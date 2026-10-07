// StreetBackdrop.cs – the Løvstakken street at night, drawn in code in three layers:
// a sky fixed to the screen, the mountain scrolling slowly behind (parallax), and the street
// itself (buildings, neon signs, streetlights, sidewalk, road) at full speed.
using Godot;

/// <summary>The whole backdrop: put this node in the level and it builds the three layers.</summary>
public partial class StreetBackdrop : Node2D
{
    [Export] public float StageLength { get; set; } = 5200f;
    /// <summary>The layers are designed for a 1152x648 screen; the camera zooms 2x, so draw at half scale.</summary>
    [Export] public float DrawScale { get; set; } = 0.5f;

    public override void _Ready()
    {
        var scale = new Vector2(DrawScale, DrawScale);
        AddChild(new SkyLayer { Name = "Sky", Scale = scale });
        AddChild(new MountainLayer { Name = "Mountain", StageLength = StageLength, Scale = scale });
        AddChild(new StreetLayer { Name = "Street", StageLength = StageLength, Scale = scale });
    }
}

/// <summary>Shared helpers: where the camera is looking.</summary>
public abstract partial class BackdropLayer : Node2D
{
    /// <summary>World x of the screen's left edge.</summary>
    protected float CameraLeft()
    {
        var cam = GetViewport()?.GetCamera2D();
        float half = ViewWidth() / 2f;
        return (cam?.GetScreenCenterPosition().X ?? half) - half;
    }

    /// <summary>How many world pixels the screen shows across (viewport width / camera zoom).</summary>
    protected float ViewWidth()
    {
        var cam = GetViewport()?.GetCamera2D();
        return PlayBounds.ScreenSize.X / (cam?.Zoom.X ?? 1f);
    }
}

/// <summary>Night sky with stars and a moon; always fills the screen.</summary>
public partial class SkyLayer : BackdropLayer
{
    public override void _Ready() => ZIndex = -300;

    public override void _Process(double delta) => Position = new Vector2(CameraLeft(), 0);

    public override void _Draw()
    {
        var top = new Color(0.04f, 0.03f, 0.10f);
        var bottom = new Color(0.22f, 0.12f, 0.30f);
        DrawPolygon(new[] { new Vector2(0, 0), new Vector2(1152, 0), new Vector2(1152, 420), new Vector2(0, 420) },
                    new[] { top, top, bottom, bottom });
        var rng = new RandomNumberGenerator { Seed = 7 };
        for (int i = 0; i < 70; i++)
            DrawCircle(new Vector2(rng.RandfRange(0, 1152), rng.RandfRange(0, 260)), rng.RandfRange(0.6f, 1.4f), new Color(1, 1, 1, rng.RandfRange(0.3f, 0.9f)));
        DrawCircle(new Vector2(930, 80), 26f, new Color(1f, 0.96f, 0.85f, 0.12f));
        DrawCircle(new Vector2(930, 80), 18f, new Color(1f, 0.96f, 0.85f));
    }
}

/// <summary>Løvstakken itself: a dark mountain ridge with a few house lights, scrolling at 15% speed.</summary>
public partial class MountainLayer : BackdropLayer
{
    [Export] public float Factor { get; set; } = 0.15f;
    [Export] public float StageLength { get; set; } = 5200f;

    public override void _Ready() => ZIndex = -200;

    public override void _Process(double delta) => Position = new Vector2(CameraLeft() * (1f - Factor), 0);

    public override void _Draw()
    {
        // Local units are screen pixels (the layer is scaled to world size).
        float width = (ViewWidth() + StageLength * Factor) / Scale.X + 400f;
        var ridge = new System.Collections.Generic.List<Vector2> { new(0, 420) };
        var rng = new RandomNumberGenerator { Seed = 11 };
        for (float x = 0; x <= width; x += 60f)
        {
            // A broad massif with a summit about a third of the way along, like Løvstakken over Bergen.
            float summit = Mathf.Exp(-Mathf.Pow((x - width * 0.3f) / (width * 0.2f), 2f)) * 190f;
            ridge.Add(new Vector2(x, 250f - summit - rng.RandfRange(0f, 18f)));
        }
        ridge.Add(new Vector2(width, 420));
        DrawColoredPolygon(ridge.ToArray(), new Color(0.10f, 0.08f, 0.18f));
        for (int i = 0; i < 40; i++)
            DrawCircle(new Vector2(rng.RandfRange(0, width), rng.RandfRange(250, 400)), 1.4f, new Color(1f, 0.85f, 0.5f, 0.7f));
    }
}

/// <summary>The street: buildings with lit windows and neon signs, streetlights, sidewalk and road.</summary>
public partial class StreetLayer : Node2D
{
    [Export] public float StageLength { get; set; } = 5200f;

    public const float SidewalkTop = 410f, CurbY = 470f;

    private static readonly string[] Signs =
        { "PUB", "KEBAB", "PIZZA", "KIOSK", "BAR", "SOLARIUM", "PANTELÅNER", "TATTOO", "BINGO", "GATEKJØKKEN", "NATTKLUBB", "SPILL" };
    private static readonly Color[] Neon =
        { new(1f, 0.3f, 0.75f), new(0.3f, 0.9f, 1f), new(0.5f, 1f, 0.4f), new(1f, 0.6f, 0.2f), new(0.85f, 0.5f, 1f) };
    private static readonly Color[] Facades =
    {
        new(0.33f, 0.17f, 0.15f), new(0.46f, 0.44f, 0.40f), new(0.45f, 0.28f, 0.14f),
        new(0.23f, 0.25f, 0.31f), new(0.38f, 0.11f, 0.11f), new(0.28f, 0.30f, 0.24f),
    };

    public override void _Ready() => ZIndex = -100;

    public override void _Draw()
    {
        var font = ThemeDB.FallbackFont;
        var rng = new RandomNumberGenerator { Seed = 1234 };
        float StageLength = this.StageLength / Scale.X; // in local units (the layer is drawn scaled)

        // Buildings.
        for (float x = -40f; x < StageLength + 40f;)
        {
            float w = rng.RandfRange(170, 310), h = rng.RandfRange(150, 270), top = SidewalkTop - h;
            var facade = Facades[rng.RandiRange(0, Facades.Length - 1)];
            DrawRect(new Rect2(x, top, w, h), facade);
            if (rng.Randf() < 0.4f)
                DrawColoredPolygon(new[] { new Vector2(x - 6, top), new Vector2(x + w + 6, top), new Vector2(x + w / 2, top - 46) }, facade.Darkened(0.35f));
            else
                DrawRect(new Rect2(x - 4, top - 6, w + 8, 8), facade.Darkened(0.4f));

            for (float wy = top + 18; wy < SidewalkTop - 80; wy += 44)
                for (float wx = x + 16; wx < x + w - 26; wx += 36)
                {
                    bool lit = rng.Randf() < 0.45f;
                    DrawRect(new Rect2(wx, wy, 16, 22), lit ? new Color(1f, 0.84f, 0.45f) : new Color(0.10f, 0.10f, 0.16f));
                    DrawRect(new Rect2(wx - 1, wy + 22, 18, 2), facade.Darkened(0.5f));
                }

            float doorX = x + rng.RandfRange(20, w - 50);
            DrawRect(new Rect2(doorX, SidewalkTop - 44, 26, 44), new Color(0.07f, 0.06f, 0.08f));
            if (rng.Randf() < 0.6f)
            {
                string text = Signs[rng.RandiRange(0, Signs.Length - 1)];
                var color = Neon[rng.RandiRange(0, Neon.Length - 1)];
                float tw = font.GetStringSize(text, HorizontalAlignment.Left, -1, 16).X + 16;
                var box = new Rect2(Mathf.Clamp(doorX - tw / 2 + 13, x + 4, x + w - tw - 4), SidewalkTop - 76, tw, 24);
                DrawRect(box.Grow(4), new Color(color, 0.12f));
                DrawRect(box, new Color(0.06f, 0.04f, 0.08f));
                DrawRect(box, color, false, 1.5f);
                DrawString(font, box.Position + new Vector2(8, 18), text, HorizontalAlignment.Left, -1, 16, color);
            }
            x += w + rng.RandfRange(0, 14); // narrow dark alleys between some buildings
        }

        // Sidewalk with slabs, then the curb.
        DrawRect(new Rect2(-40, SidewalkTop, StageLength + 80, CurbY - SidewalkTop), new Color(0.33f, 0.33f, 0.37f));
        for (float x = 0; x < StageLength; x += 64)
            DrawLine(new Vector2(x, SidewalkTop), new Vector2(x - 18, CurbY), new Color(0.28f, 0.28f, 0.32f), 1f);
        DrawRect(new Rect2(-40, CurbY - 2, StageLength + 80, 6), new Color(0.52f, 0.52f, 0.56f));

        // Road with a dashed yellow centre line.
        DrawRect(new Rect2(-40, CurbY + 4, StageLength + 80, 648 - CurbY), new Color(0.15f, 0.15f, 0.18f));
        for (float x = 0; x < StageLength; x += 90)
            DrawRect(new Rect2(x, 566, 48, 4), new Color(0.85f, 0.7f, 0.2f));

        // Streetlights with pools of light.
        for (float x = 380; x < StageLength; x += 720)
        {
            DrawSetTransform(new Vector2(x + 24, 450), 0f, new Vector2(1f, 0.25f));
            DrawCircle(Vector2.Zero, 80f, new Color(1f, 0.85f, 0.5f, 0.08f));
            DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
            DrawLine(new Vector2(x, SidewalkTop + 6), new Vector2(x, 240), new Color(0.2f, 0.2f, 0.24f), 4f);
            DrawLine(new Vector2(x, 242), new Vector2(x + 24, 242), new Color(0.2f, 0.2f, 0.24f), 3f);
            DrawCircle(new Vector2(x + 24, 248), 22f, new Color(1f, 0.85f, 0.5f, 0.15f));
            DrawRect(new Rect2(x + 16, 242, 16, 6), new Color(1f, 0.9f, 0.6f));
        }

        // Street sign at the end of the stage.
        float sx = StageLength - 260;
        DrawLine(new Vector2(sx, SidewalkTop + 4), new Vector2(sx, 330), new Color(0.6f, 0.6f, 0.62f), 3f);
        DrawRect(new Rect2(sx - 70, 300, 140, 30), new Color(0.1f, 0.3f, 0.6f));
        DrawString(font, new Vector2(sx - 62, 321), "Løvstakkveien", HorizontalAlignment.Left, -1, 16, Colors.White);
    }
}
