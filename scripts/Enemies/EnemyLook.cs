// EnemyLook.cs – draws each enemy type in code (same 40 px frame as the Pimp: feet at y = 20).
// Faces right; ClassLook flips it. The front arm swings on attacks and holds the type's prop.
// Skin tone is picked per enemy from a varied palette, independent of type.
using System;
using Godot;

public partial class EnemyLook : ClassLook
{
    public EnemyType Type { get; set; }
    public int Seed { get; set; }

    private static readonly Color[] SkinTones =
    {
        new(0.96f, 0.80f, 0.69f), new(0.86f, 0.66f, 0.52f), new(0.74f, 0.52f, 0.36f),
        new(0.55f, 0.36f, 0.24f), new(0.38f, 0.25f, 0.17f),
    };
    private static readonly Color Gold = new(1.00f, 0.80f, 0.18f);
    private static readonly Color Ink = new(0.07f, 0.06f, 0.07f);
    private static readonly Vector2 Shoulder = new(5f, -9f);

    private Color _skin;
    private float _armAngle;
    private Tween _tween;
    private float _twitch;

    /// <summary>Front arm rotation (radians); negative swings forward.</summary>
    public float ArmAngle
    {
        get => _armAngle;
        set { _armAngle = value; QueueRedraw(); }
    }

    public override void _Ready() => _skin = SkinTones[Math.Abs(Seed) % SkinTones.Length];

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Type?.Id != "dope_fiend") return;
        // Can't stand still: random little twitches.
        _twitch -= (float)delta;
        if (_twitch <= 0f)
        {
            _twitch = 0.05f + GD.Randf() * 0.15f;
            Position += new Vector2(GD.Randf() - 0.5f, GD.Randf() - 0.5f) * 1.8f;
        }
    }

    public override void PlayAttack()
    {
        _tween?.Kill();
        _tween = CreateTween();
        _tween.TweenMethod(Callable.From<float>(a => ArmAngle = a), 0f, 0.7f, 0.05f);
        _tween.TweenMethod(Callable.From<float>(a => ArmAngle = a), 0.7f, -1.5f, 0.08f);
        _tween.TweenMethod(Callable.From<float>(a => ArmAngle = a), -1.5f, 0f, 0.2f);
    }

    private static Vector2 V(float x, float y) => new(x, y);

    public override void _Draw()
    {
        switch (Type?.Id)
        {
            case "skank": DrawSkank(); break;
            case "thug": DrawThug(); break;
            case "playa": DrawPlaya(); break;
            case "drug_dealer": DrawDealer(); break;
            case "baller": DrawBaller(); break;
            case "g": DrawG(); break;
            case "dope_fiend": DrawFiend(); break;
            default: DrawHustler(); break;
        }
    }

    // ---------- shared body parts ----------

    private void Shadow()
    {
        DrawSetTransform(V(0, 20), 0f, V(1f, 0.3f));
        DrawCircle(Vector2.Zero, 11f, new Color(0, 0, 0, 0.35f));
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
    }

    private void Legs(Color c, float flare) =>
        DrawColoredPolygon(new[] { V(-6, 2), V(6, 2), V(6 + flare, 11), V(5.5f, 17.5f), V(1.2f, 17.5f), V(0.4f, 9), V(-0.4f, 9), V(-1.2f, 17.5f), V(-5.5f, 17.5f), V(-6 - flare, 11) }, c);

    private void Shoes(Color c, Color? accent = null)
    {
        DrawColoredPolygon(new[] { V(-7, 17.5f), V(-1, 17.5f), V(0, 20.5f), V(-8.5f, 20.5f) }, c);
        DrawColoredPolygon(new[] { V(1, 17.5f), V(7, 17.5f), V(10, 20.5f), V(1, 20.5f) }, c);
        if (accent is { } a)
        {
            DrawRect(new Rect2(-7.5f, 19.4f, 7f, 1.1f), a);
            DrawRect(new Rect2(1f, 19.4f, 8.5f, 1.1f), a);
        }
    }

    private void Torso(Color c, float half, float bottom) =>
        DrawColoredPolygon(new[] { V(-half, -12), V(half, -12), V(half - 0.5f, bottom), V(-half + 0.5f, bottom) }, c);

    private void Head(Vector2 o = default)
    {
        DrawRect(new Rect2(-1.5f + o.X, -14 + o.Y, 4, 3), _skin);
        DrawCircle(V(1, -17.5f) + o, 5f, _skin);
        DrawCircle(V(3.4f, -18.3f) + o, 0.8f, Ink);
    }

    private void Shades(Color frame)
    {
        DrawColoredPolygon(new[] { V(1.8f, -19.4f), V(6.2f, -19.4f), V(5.8f, -17.5f), V(2.2f, -17.5f) }, Ink);
        DrawLine(V(1.6f, -19.4f), V(6.3f, -19.4f), frame, 0.7f);
    }

    /// <summary>The front arm, rotated about the shoulder; <paramref name="prop"/> draws in arm space.</summary>
    private void Arm(Color sleeve, Action prop = null, Color? hand = null)
    {
        DrawSetTransform(Shoulder, _armAngle, Vector2.One);
        DrawLine(V(0, 0), V(3.8f, 8), sleeve, 4.2f);
        DrawCircle(V(4.4f, 9.2f), 2.1f, hand ?? _skin);
        prop?.Invoke();
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
    }

    // ---------- the roster ----------

    /// <summary>Street Hustler: flat cap, cheap leather jacket, a sleeve full of "genuine" gold watches.</summary>
    private void DrawHustler()
    {
        var leather = new Color(0.42f, 0.25f, 0.13f);
        Shadow();
        Legs(new Color(0.36f, 0.36f, 0.4f), 0.5f);
        Shoes(new Color(0.35f, 0.2f, 0.1f));
        Torso(leather, 8.5f, 6);
        DrawColoredPolygon(new[] { V(-2.5f, -12), V(2.5f, -12), V(0, -5) }, new Color(0.9f, 0.9f, 0.85f));
        DrawColoredPolygon(new[] { V(-2.5f, -12), V(-5, -12), V(0, -3.5f) }, new Color(0.3f, 0.17f, 0.08f));
        DrawColoredPolygon(new[] { V(2.5f, -12), V(5, -12), V(0, -3.5f) }, new Color(0.3f, 0.17f, 0.08f));
        Head();
        DrawLine(V(2.2f, -15.6f), V(5, -15.6f), Ink, 0.9f); // pencil mustache
        DrawColoredPolygon(new[] { V(-4.3f, -20.5f), V(5.8f, -20.5f), V(8.8f, -19.5f), V(6.5f, -22.8f), V(1, -24), V(-3.8f, -22.6f) }, new Color(0.45f, 0.45f, 0.48f));
        Arm(leather, () =>
        {
            foreach (var p in new[] { V(0.6f, 2.2f), V(1.5f, 4.1f), V(2.4f, 6f) })
                DrawRect(new Rect2(p, V(2.8f, 1f)), Gold);
        });
    }

    /// <summary>Skank: big teased blonde hair, gold hoops, leopard-print jacket, pink skirt, swinging handbag.</summary>
    private void DrawSkank()
    {
        var blonde = new Color(0.98f, 0.86f, 0.5f);
        var leopard = new Color(0.88f, 0.6f, 0.25f);
        var spot = new Color(0.3f, 0.17f, 0.07f);
        Shadow();
        Legs(new Color(0.1f, 0.08f, 0.1f), -1f);
        Shoes(new Color(0.8f, 0.1f, 0.15f));
        DrawColoredPolygon(new[] { V(-6.5f, 0), V(6.5f, 0), V(8, 8), V(-8, 8) }, new Color(0.95f, 0.35f, 0.6f));
        Torso(leopard, 7.5f, 1);
        foreach (var p in new[] { V(-5, -9), V(-2, -5), V(-5.5f, -2), V(4, -9.5f), V(5.5f, -4), V(1.5f, -1.5f), V(-1, -10) })
            DrawCircle(p, 0.9f, spot);
        foreach (var (p, r) in new[] { (V(-2.5f, -19), 6f), (V(3, -21.5f), 4.8f), (V(-5, -15), 4f), (V(6, -19.5f), 3.2f) })
            DrawCircle(p, r, blonde);
        Head();
        DrawRect(new Rect2(4.1f, -15.7f, 1.7f, 1f), new Color(0.85f, 0.05f, 0.2f)); // lipstick
        DrawLine(V(2.7f, -19.4f), V(4.4f, -19.6f), Ink, 0.6f); // lashes
        DrawCircle(V(2, -22), 4f, blonde); // bangs
        DrawArc(V(-1.6f, -14.2f), 1.7f, 0f, Mathf.Tau, 12, Gold, 0.8f); // hoop
        Arm(leopard, () =>
        {
            DrawLine(V(4.4f, 9.2f), V(4.2f, 11.5f), Ink, 0.7f);
            DrawRect(new Rect2(1.5f, 11.5f, 6.5f, 5f), new Color(0.75f, 0.1f, 0.2f));
            DrawRect(new Rect2(4.2f, 11.5f, 1.2f, 1.2f), Gold);
        });
    }

    /// <summary>Thug: big, hood up, scowl, baseball bat.</summary>
    private void DrawThug()
    {
        var hoodie = new Color(0.13f, 0.13f, 0.15f);
        Shadow();
        Legs(new Color(0.16f, 0.2f, 0.32f), 1.5f);
        Shoes(new Color(0.92f, 0.92f, 0.92f));
        Torso(hoodie, 10, 6);
        DrawLine(V(-5, 1.5f), V(5, 1.5f), new Color(0.22f, 0.22f, 0.25f), 0.8f); // pocket
        DrawLine(V(0.3f, -11), V(0.3f, -6.5f), new Color(0.85f, 0.85f, 0.85f), 0.6f); // drawstrings
        DrawLine(V(2, -11), V(2, -7), new Color(0.85f, 0.85f, 0.85f), 0.6f);
        DrawCircle(V(0.5f, -18), 6.6f, hoodie); // hood
        DrawCircle(V(2, -17.2f), 4.3f, _skin);
        DrawCircle(V(4, -18f), 0.8f, Ink);
        DrawLine(V(2.8f, -19.6f), V(5.3f, -18.8f), Ink, 0.9f); // scowl
        Arm(hoodie, () =>
        {
            var wood = new Color(0.62f, 0.43f, 0.22f);
            DrawLine(V(4.6f, 10.5f), V(6, 2), wood, 1.8f);
            DrawLine(V(6, 2), V(8.8f, -13), wood, 3.4f);
            DrawCircle(V(4.5f, 11.2f), 1.3f, wood);
        });
    }

    /// <summary>Playa: white suit, open pink shirt, gold chain, slick hair, shades, a rose for the ladies.</summary>
    private void DrawPlaya()
    {
        var white = new Color(0.95f, 0.95f, 0.92f);
        Shadow();
        Legs(white, 0.8f);
        Shoes(new Color(0.12f, 0.1f, 0.1f));
        Torso(white, 8.5f, 6);
        DrawColoredPolygon(new[] { V(-3.5f, -12), V(3.5f, -12), V(0, -2) }, new Color(1f, 0.55f, 0.72f));
        DrawColoredPolygon(new[] { V(-1.6f, -12), V(1.6f, -12), V(0, -7) }, _skin);
        DrawLine(V(-3.5f, -12), V(0, -2), new Color(0.78f, 0.78f, 0.75f), 0.8f);
        DrawLine(V(3.5f, -12), V(0, -2), new Color(0.78f, 0.78f, 0.75f), 0.8f);
        DrawArc(V(0, -13.5f), 5f, 0.5f, Mathf.Pi - 0.5f, 12, Gold, 1f);
        DrawCircle(V(0.5f, -18.8f), 5.4f, new Color(0.08f, 0.06f, 0.05f)); // slicked hair
        DrawCircle(V(1.4f, -17.2f), 4.8f, _skin);
        Shades(Gold);
        DrawLine(V(2.5f, -15.3f), V(5, -15.9f), Ink, 0.7f); // smirk
        Arm(white, () =>
        {
            DrawLine(V(4.4f, 9.2f), V(7, 2), new Color(0.2f, 0.55f, 0.2f), 0.8f);
            DrawCircle(V(7.2f, 1.5f), 1.9f, new Color(0.85f, 0.1f, 0.15f));
            DrawCircle(V(7.2f, 1.5f), 0.8f, new Color(0.55f, 0.05f, 0.1f));
        });
    }

    /// <summary>Drug Dealer: olive puffer jacket, backwards red cap, cross-body bag, a bottle ready to throw.</summary>
    private void DrawDealer()
    {
        var puffer = new Color(0.32f, 0.38f, 0.2f);
        var quilt = new Color(0.24f, 0.29f, 0.14f);
        var red = new Color(0.8f, 0.12f, 0.12f);
        Shadow();
        Legs(new Color(0.55f, 0.55f, 0.58f), 0.8f);
        Shoes(new Color(0.92f, 0.92f, 0.92f), red);
        Torso(puffer, 9.8f, 4);
        foreach (float y in new[] { -8f, -4f, 0f }) DrawLine(V(-9.5f, y), V(9.5f, y), quilt, 0.8f);
        DrawLine(V(0, -12), V(0, 4), quilt, 0.6f);
        DrawLine(V(-8, -11), V(7, 1), Ink, 1.2f); // bag strap
        DrawRect(new Rect2(2, -2, 6, 4), Ink);
        Head();
        DrawColoredPolygon(new[] { V(-4, -19.5f), V(6, -19.5f), V(5, -22.8f), V(1, -24), V(-2.8f, -23) }, red);
        DrawColoredPolygon(new[] { V(-4, -19.5f), V(-8.5f, -19), V(-8, -20.4f), V(-3.6f, -21) }, new Color(0.6f, 0.08f, 0.08f));
        Arm(puffer, () =>
        {
            var glass = new Color(0.2f, 0.55f, 0.25f);
            DrawRect(new Rect2(5, 4.5f, 2.6f, 5), glass);
            DrawRect(new Rect2(5.7f, 2.4f, 1.2f, 2.2f), glass);
        });
    }

    /// <summary>Baller: fur coat, heavy gold chains, gold shades and sneakers, a fan of cash.</summary>
    private void DrawBaller()
    {
        var fur = new Color(0.93f, 0.88f, 0.78f);
        var furLight = new Color(0.98f, 0.95f, 0.88f);
        Shadow();
        Legs(new Color(0.08f, 0.08f, 0.1f), 0.6f);
        Shoes(Gold, new Color(1f, 1f, 1f));
        Torso(new Color(0.08f, 0.08f, 0.1f), 7, 2);
        DrawColoredPolygon(new[] { V(-11.5f, -12.5f), V(-3, -12.5f), V(-2, 9), V(-11, 9) }, fur);
        DrawColoredPolygon(new[] { V(3, -12.5f), V(11.5f, -12.5f), V(11, 9), V(2, 9) }, fur);
        foreach (var p in new[] { V(-5.5f, -12), V(-3, -11.2f), V(3, -11.2f), V(5.5f, -12) }) DrawCircle(p, 2.4f, furLight);
        for (float x = -10.5f; x <= 10.5f; x += 3f)
            if (Mathf.Abs(x) > 2f) DrawCircle(V(x, 9), 1.6f, furLight);
        DrawArc(V(0, -14), 6f, 0.4f, Mathf.Pi - 0.4f, 14, Gold, 2f);
        DrawArc(V(0, -14.5f), 9f, 0.5f, Mathf.Pi - 0.5f, 18, Gold, 2f);
        DrawCircle(V(0, -5.3f), 2.6f, Gold);
        Head();
        DrawArc(V(1, -17.5f), 5f, Mathf.Pi + 0.3f, Mathf.Tau - 0.3f, 10, new Color(0.1f, 0.07f, 0.05f), 2f); // short hair
        Shades(Gold);
        Arm(fur, () =>
        {
            DrawColoredPolygon(new[] { V(3, 8), V(9, 6.5f), V(9.5f, 9.5f), V(3.5f, 11) }, new Color(0.35f, 0.65f, 0.3f));
            DrawColoredPolygon(new[] { V(3.5f, 9), V(9.5f, 9), V(9.2f, 12), V(3.5f, 12) }, new Color(0.45f, 0.75f, 0.38f));
        });
    }

    /// <summary>G: white tank top, tattooed arms, blue bandana, shades, baggy khakis.</summary>
    private void DrawG()
    {
        var tank = new Color(0.96f, 0.96f, 0.96f);
        var bandana = new Color(0.15f, 0.3f, 0.75f);
        Shadow();
        Legs(new Color(0.78f, 0.69f, 0.5f), 2f);
        Shoes(new Color(0.1f, 0.1f, 0.1f));
        DrawCircle(V(-7, -10.5f), 2.4f, _skin); // back shoulder
        Torso(tank, 6.5f, 4);
        DrawRect(new Rect2(-6.5f, 2, 13, 1.4f), Ink); // belt
        Head();
        DrawRect(new Rect2(3.6f, -13.9f, 1.4f, 1.2f), Ink); // goatee
        DrawColoredPolygon(new[] { V(-4.2f, -19), V(6, -19.4f), V(5.5f, -22.5f), V(1, -23.6f), V(-3.5f, -22.5f) }, bandana);
        DrawColoredPolygon(new[] { V(-4, -19.5f), V(-7.5f, -17), V(-6.8f, -16), V(-3.8f, -18.2f) }, bandana);
        DrawColoredPolygon(new[] { V(-4, -20.5f), V(-8, -20), V(-7.8f, -18.8f), V(-3.9f, -19.2f) }, bandana);
        Shades(Ink);
        Arm(_skin, () =>
        {
            DrawLine(V(0.8f, 2), V(2.2f, 4.2f), new Color(0.15f, 0.2f, 0.35f), 0.8f); // tattoos
            DrawLine(V(1.8f, 5.4f), V(3, 7), new Color(0.15f, 0.2f, 0.35f), 0.8f);
        });
    }

    /// <summary>Dope Fiend: hunched, ragged patched hoodie, torn jeans, messy hair, dark-ringed eyes, twitchy.</summary>
    private void DrawFiend()
    {
        var hoodie = new Color(0.42f, 0.4f, 0.37f);
        var hunch = V(2, 2.2f);
        Shadow();
        Legs(new Color(0.45f, 0.55f, 0.7f), 0.4f);
        DrawCircle(V(-3.5f, 12), 1.3f, _skin); // torn knee
        Shoes(new Color(0.35f, 0.33f, 0.3f));
        DrawColoredPolygon(new[] { V(-8, -12), V(8, -12), V(8, 6), V(6, 8), V(4, 6), V(2, 8.5f), V(-1, 6), V(-3, 8), V(-5.5f, 6), V(-8, 7.5f) }, hoodie);
        DrawRect(new Rect2(-6, -4, 3.5f, 3.5f), new Color(0.45f, 0.3f, 0.18f)); // patch
        DrawLine(V(-6, -4), V(-2.5f, -0.5f), new Color(0.25f, 0.2f, 0.15f), 0.5f);
        Head(hunch);
        DrawCircle(V(3.4f, -18.3f) + hunch, 1.6f, new Color(0.35f, 0.25f, 0.35f)); // dark rings
        DrawCircle(V(3.4f, -18.3f) + hunch, 0.7f, Ink);
        DrawLine(V(2.5f, -15.4f) + hunch, V(4.8f, -15f) + hunch, Ink, 0.6f);
        var hair = new Color(0.25f, 0.18f, 0.1f);
        foreach (var (a, b) in new[] { (V(-3, -21), V(-6, -24.5f)), (V(-0.5f, -22.3f), V(-1.5f, -26.5f)), (V(2, -22.5f), V(3.5f, -26)), (V(4.5f, -21), V(7.5f, -23.5f)), (V(-4, -19), V(-7.5f, -19.5f)) })
            DrawLine(a + hunch, b + hunch, hair, 1.4f);
        Arm(hoodie);
    }
}
