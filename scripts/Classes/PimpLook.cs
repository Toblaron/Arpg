// PimpLook.cs – the Pimp, drawn in code: dark violet zoot suit, feathered dapper hat to match,
// gold chains and rings, and a huge pimp cane. Faces right; ClassLook flips it.
// Origin = the character's centre; feet at y = 20 (matches the 40 px collision capsule).
using Godot;

public partial class PimpLook : ClassLook
{
    [Export] public Color SkinTone { get; set; } = new(0.86f, 0.66f, 0.52f);

    public static readonly Color Suit = new(0.30f, 0.07f, 0.42f);
    public static readonly Color SuitDark = new(0.19f, 0.04f, 0.28f);
    public static readonly Color SuitStripe = new(0.45f, 0.18f, 0.60f);
    public static readonly Color Gold = new(1.00f, 0.80f, 0.18f);
    public static readonly Color GoldDark = new(0.75f, 0.55f, 0.08f);
    public static readonly Color Feather = new(0.80f, 0.58f, 1.00f);
    public static readonly Color Shirt = new(0.96f, 0.94f, 0.90f);
    public static readonly Color Shoe = new(0.08f, 0.06f, 0.08f);

    private CaneArm _arm;
    private Tween _tween;

    public override void _Ready()
    {
        _arm = new CaneArm { Name = "CaneArm", Position = new Vector2(6, -9), SkinTone = SkinTone };
        AddChild(_arm);
    }

    private static Vector2 V(float x, float y) => new(x, y);

    public override void _Draw()
    {
        // Shadow on the floor.
        DrawSetTransform(V(0, 20), 0f, V(1f, 0.3f));
        DrawCircle(Vector2.Zero, 11f, new Color(0, 0, 0, 0.35f));
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One);

        // Zoot trousers: high waist, wide at the knee, pegged at the ankle.
        DrawColoredPolygon(new[] { V(-7, 2), V(7, 2), V(9.5f, 10), V(6, 18), V(1.5f, 18), V(0.5f, 10), V(-0.5f, 10), V(-1.5f, 18), V(-6, 18), V(-9.5f, 10) }, Suit);
        DrawLine(V(0, 4), V(0, 10), SuitDark, 1f);

        // Two-tone shoes with white spats.
        DrawColoredPolygon(new[] { V(-7, 17.5f), V(-1, 17.5f), V(0, 20.5f), V(-8.5f, 20.5f) }, Shoe);
        DrawColoredPolygon(new[] { V(1, 17.5f), V(7, 17.5f), V(10.5f, 20.5f), V(1, 20.5f) }, Shoe);
        DrawRect(new Rect2(-6.5f, 17.5f, 4.5f, 1.5f), Shirt);
        DrawRect(new Rect2(1.5f, 17.5f, 4.5f, 1.5f), Shirt);

        // Long drape jacket with padded shoulders.
        DrawColoredPolygon(new[] { V(-10.5f, -12), V(10.5f, -12), V(9.5f, 7), V(4, 9), V(0, 4), V(-4, 9), V(-9.5f, 7) }, Suit);
        foreach (float x in new[] { -8f, -5.5f, 5.5f, 8f })
            DrawLine(V(x, -11), V(x * 0.95f, 6), SuitStripe, 0.6f);

        // Shirt, gold tie, lapels.
        DrawColoredPolygon(new[] { V(-3.5f, -12), V(3.5f, -12), V(0, -3) }, Shirt);
        DrawColoredPolygon(new[] { V(0, -11), V(1.2f, -6.5f), V(0, -4.5f), V(-1.2f, -6.5f) }, Gold);
        DrawColoredPolygon(new[] { V(-3.5f, -12), V(-6, -12), V(0, -1.5f), V(-0.6f, -3) }, SuitDark);
        DrawColoredPolygon(new[] { V(3.5f, -12), V(6, -12), V(0, -1.5f), V(0.6f, -3) }, SuitDark);

        // Gold chains, the long one with a medallion.
        DrawArc(V(0, -14), 6.5f, 0.45f, Mathf.Pi - 0.45f, 14, Gold, 1.2f);
        DrawArc(V(0, -14.5f), 9.5f, 0.55f, Mathf.Pi - 0.55f, 18, Gold, 1.2f);
        DrawCircle(V(0, -4.8f), 2.2f, Gold);
        DrawCircle(V(0, -4.8f), 1.1f, GoldDark);

        // Head: neck, face, a smug grin with a gold tooth.
        DrawRect(new Rect2(-1.5f, -14, 4, 3), SkinTone);
        DrawCircle(V(1, -17.5f), 5f, SkinTone);
        DrawCircle(V(3.4f, -18.3f), 0.8f, Shoe);
        DrawLine(V(2, -15.2f), V(5, -15.8f), Shoe, 0.8f);
        DrawRect(new Rect2(3.6f, -15.7f, 0.9f, 0.9f), Gold);

        // Dapper hat to match the suit: crown with a pinch, gold band, wide brim, plume.
        DrawColoredPolygon(new[] { V(-4.5f, -21), V(7, -21), V(6, -30), V(1.2f, -28.2f), V(-3.6f, -30) }, Suit);
        DrawRect(new Rect2(-4.6f, -23.6f, 11.8f, 2.4f), Gold);
        DrawSetTransform(V(1.2f, -21), 0f, V(1f, 0.2f));
        DrawCircle(Vector2.Zero, 11.5f, SuitDark);
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
        DrawColoredPolygon(new[] { V(-3.5f, -23), V(-7, -29), V(-12.5f, -38), V(-10.5f, -30.5f), V(-6, -22.5f) }, Feather);
        DrawLine(V(-4.5f, -23), V(-12, -37), GoldDark, 0.7f);
    }

    public override void PlayAttack() => SwingArm(1.3f, -1.9f, 0.3f); // Cane Sweep: back over the shoulder, then wide through the front

    public override void PlayAbility()
    {
        // Big wind-up and a full backhand, with a squash on impact.
        SwingArm(1.1f, -2.3f, 0.36f);
        var squash = CreateTween();
        squash.TweenProperty(this, "scale:y", 0.88f, 0.08f).SetDelay(0.14f);
        squash.TweenProperty(this, "scale:y", 1f, 0.14f);
    }

    private void SwingArm(float windUp, float follow, float duration)
    {
        if (_arm == null) return;
        _tween?.Kill();
        _tween = CreateTween();
        // Hits land the moment the key is pressed, so the wind-up is a flick and the
        // forward swing reaches its peak right as the impact effect shows.
        _tween.TweenProperty(_arm, "rotation", windUp, duration * 0.12f).SetEase(Tween.EaseType.Out);
        _tween.TweenProperty(_arm, "rotation", follow, duration * 0.2f).SetEase(Tween.EaseType.In);
        _tween.TweenInterval(duration * 0.15f);
        _tween.TweenProperty(_arm, "rotation", 0f, duration * 0.53f).SetEase(Tween.EaseType.InOut);
    }

    /// <summary>The cane arm, pivoting at the shoulder so attacks can swing it.</summary>
    private partial class CaneArm : Node2D
    {
        public Color SkinTone { get; set; }

        public override void _Draw()
        {
            // Sleeve and hand with a gold ring.
            DrawLine(new Vector2(0, 0), new Vector2(3.8f, 8), Suit, 4.2f);
            DrawCircle(new Vector2(4.4f, 9.2f), 2.1f, SkinTone);
            DrawRect(new Rect2(3.6f, 8.6f, 1.6f, 1.2f), Gold);

            // The huge cane: from above the shoulder to the floor, gold knob and tip.
            DrawLine(new Vector2(6.8f, -10), new Vector2(2.2f, 29), SuitDark, 2.2f);
            DrawCircle(new Vector2(6.9f, -11), 3.4f, Gold);
            DrawCircle(new Vector2(6.0f, -12), 1.2f, new Color(1f, 0.95f, 0.7f));
            DrawRect(new Rect2(1.2f, 26.5f, 2.2f, 2.5f), Gold);
        }
    }
}
