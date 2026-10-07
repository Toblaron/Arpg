// Entity.cs
using Godot;
using System;

public partial class Entity : Node2D
{
    private const float Y_TOLERANCE = 0.5f; // ignore sub‑pixel changes
    private float _lastY = float.MinValue;

    public override void _PhysicsProcess(double delta)
    {
        float curY = GlobalPosition.Y;
        if (Mathf.Abs(curY - _lastY) > Y_TOLERANCE)
        {
            // Godot’s built‑in ZIndex is already optimal.
            ZIndex = -(int)Math.Floor(curY);
            _lastY = curY;
        }
    }
}
