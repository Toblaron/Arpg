// Item.cs
using Godot;

public partial class Item : Area2D
{
    public override void _Ready()
    {
        Connect("body_entered", this, nameof(OnPicked));
    }

    private void OnPicked(Node body)
    {
        if (body.IsInGroup("Player"))
        {
            GD.Print($"Picked up {GetMeta("item_id")}");
            QueueFree();
        }
    }
}
