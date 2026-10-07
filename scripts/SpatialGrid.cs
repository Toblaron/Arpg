// SpatialGrid.cs – a uniform grid for fast "who is near this point" queries (hit checks).
// Re-register moving things each physics frame with Rebuild(), or Add/Remove as they spawn/die.
using System.Collections.Generic;
using Godot;

public partial class SpatialGrid : Node
{
    [Export] public int CellSize { get; set; } = 64;

    private readonly Dictionary<Vector2I, List<Node2D>> _cells = new();

    private Vector2I CellOf(Vector2 p) => new(Mathf.FloorToInt(p.X / CellSize), Mathf.FloorToInt(p.Y / CellSize));

    public void Add(Node2D node)
    {
        var key = CellOf(node.GlobalPosition);
        if (!_cells.TryGetValue(key, out var list)) _cells[key] = list = new List<Node2D>();
        list.Add(node);
    }

    public void Remove(Node2D node)
    {
        var key = CellOf(node.GlobalPosition);
        if (_cells.TryGetValue(key, out var list) && list.Remove(node) && list.Count == 0)
            _cells.Remove(key);
    }

    /// <summary>Re-bucket everything in a node group (e.g. "Enemy") at its current position.</summary>
    public void Rebuild(string group)
    {
        _cells.Clear();
        foreach (var n in GetTree().GetNodesInGroup(group))
            if (n is Node2D node) Add(node);
    }

    /// <summary>Everything within <paramref name="radius"/> of <paramref name="center"/>.</summary>
    public List<Node2D> Query(Vector2 center, float radius)
    {
        var results = new List<Node2D>();
        Vector2I min = CellOf(center - new Vector2(radius, radius)), max = CellOf(center + new Vector2(radius, radius));
        for (int x = min.X; x <= max.X; x++)
        for (int y = min.Y; y <= max.Y; y++)
        {
            if (!_cells.TryGetValue(new Vector2I(x, y), out var list)) continue;
            foreach (var n in list)
                if (IsInstanceValid(n) && n.GlobalPosition.DistanceTo(center) <= radius) results.Add(n);
        }
        return results;
    }
}
