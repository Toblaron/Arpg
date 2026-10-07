// LootDrop.cs – put on an enemy (child named "LootDrop"); rolls and scatters items when it dies.
using Godot;

public partial class LootDrop : Node
{
    [Export] public int MonsterLevel { get; set; } = 1;     // item level of everything it drops
    [Export(PropertyHint.Range, "0,1,0.01")] public float DropChance { get; set; } = 0.6f;
    [Export] public int MaxDrops { get; set; } = 2;
    [Export] public float ScatterRadius { get; set; } = 24f;
    /// <summary>Optional custom pickup scene (root must be an <see cref="Item"/>); a default is built otherwise.</summary>
    [Export] public PackedScene ItemScene { get; set; }

    public void Drop(Vector2 at)
    {
        var world = GetTree().CurrentScene;
        if (world == null) return;
        float magicFind = (GetTree().GetFirstNodeInGroup("Player") as Player)?.Equipment?.GetStat(StatKey.MagicFind) ?? 0f;

        for (int i = 0; i < MaxDrops; i++)
        {
            if (GD.Randf() > DropChance) continue;
            var item = ItemGenerator.Generate(MonsterLevel, magicFind);
            var pickup = ItemScene?.Instantiate<Item>() ?? new Item();
            pickup.Setup(item);
            pickup.Position = at + Vector2.FromAngle(GD.Randf() * Mathf.Tau) * GD.Randf() * ScatterRadius;
            world.CallDeferred(Node.MethodName.AddChild, pickup); // we're inside a physics callback
        }
    }
}
