// Enemy.cs – add at end of Attack or when health reaches 0
public void Die()
{
    EmitSignal(nameof(Killed));
    var loot = new LootDrop { ItemScene = GD.Load<PackedScene>("res://Item.tscn") };
    AddChild(loot);
    loot.Spawn();
    QueueFree();
}
[Signal] public delegate void KilledEventHandler();
