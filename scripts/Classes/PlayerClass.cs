// PlayerClass.cs – a playable class: its name, its unique ability and how it looks.
// Add a class by subclassing PlayerClass and adding its id to Create().
using System;
using Godot;

public abstract class PlayerClass
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public abstract string AbilityName { get; }
    public abstract float AbilityCooldown { get; }

    /// <summary>Runs the class's unique ability (bound to the "ability" input action).</summary>
    public abstract void UseAbility(Player player);

    /// <summary>The character's look; replaces the placeholder "Body" node. Null keeps the placeholder.</summary>
    public virtual ClassLook CreateLook() => null;

    public static PlayerClass Create(string id) => id switch
    {
        PimpClass.ClassId => new PimpClass(),
        _ => throw new ArgumentException($"Unknown player class '{id}'"),
    };
}

/// <summary>Base for a class's drawn character. Faces right by default; flipped for left.</summary>
public partial class ClassLook : Node2D
{
    private float _bobTime;
    private bool _moving;

    /// <summary>Overall size (the boss is drawn bigger); facing flips it horizontally.</summary>
    public float BaseScale { get; set; } = 1f;

    public void SetFacing(float dirX)
    {
        if (dirX != 0f) Scale = new Vector2(Mathf.Sign(dirX) * BaseScale, BaseScale);
    }

    public void SetMoving(bool moving) => _moving = moving;

    public override void _Process(double delta)
    {
        // Small strut bob while walking.
        _bobTime = _moving ? _bobTime + (float)delta : 0f;
        Position = new Vector2(0f, _moving ? -Mathf.Abs(Mathf.Sin(_bobTime * 12f)) * 1.5f * BaseScale : 0f);
    }

    public virtual void PlayAttack() { }
    public virtual void PlayAbility() { }
}
