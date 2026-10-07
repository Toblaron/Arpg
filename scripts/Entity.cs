// Entity.cs – base for static or scripted world objects that need depth sorting.
using Godot;

public partial class Entity : Node2D
{
    private float _lastY = float.MinValue;

    public override void _PhysicsProcess(double delta) => YSort.Update(this, ref _lastY);
}

/// <summary>
/// Beat-'em-up depth: whatever is lower on screen is drawn in front. Only touches ZIndex when
/// the Y position actually changes, so idle objects cost nothing.
/// </summary>
public static class YSort
{
    private const float Tolerance = 0.5f; // ignore sub-pixel jitter

    public static void Update(CanvasItem item, ref float lastY)
    {
        float y = item is Node2D n ? n.GlobalPosition.Y : 0f;
        if (Mathf.Abs(y - lastY) <= Tolerance) return;
        // ZIndex is limited to ±4096 in Godot.
        item.ZIndex = Mathf.Clamp(Mathf.FloorToInt(y), (int)RenderingServer.CanvasItemZMin, (int)RenderingServer.CanvasItemZMax);
        lastY = y;
    }
}
