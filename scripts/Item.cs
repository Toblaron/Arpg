// Item.cs – an item lying on the ground. Shows its name in rarity colour (D2 style); the
// player picks it up by walking over it.
using Godot;

public partial class Item : Area2D
{
    [Signal] public delegate void PickedUpEventHandler(Node by);

    public ItemInstance Data { get; private set; }
    private Label _label;

    public void Setup(ItemInstance data)
    {
        Data = data;
        if (_label != null) Refresh();
    }

    public override void _Ready()
    {
        if (GetNodeOrNull<CollisionShape2D>("CollisionShape2D") == null)
            AddChild(new CollisionShape2D { Name = "CollisionShape2D", Shape = new CircleShape2D { Radius = 12f } });
        _label = GetNodeOrNull<Label>("Label");
        if (_label == null)
        {
            _label = new Label { Name = "Label", HorizontalAlignment = HorizontalAlignment.Center, Position = new Vector2(-60, -28), Size = new Vector2(120, 20) };
            _label.AddThemeFontSizeOverride("font_size", 11);
            AddChild(_label);
        }
        Refresh();
        BodyEntered += OnBodyEntered;
    }

    private void Refresh()
    {
        if (Data == null) return;
        _label.Text = Data.Name;
        _label.Modulate = Data.RarityColor;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (Data == null || !body.IsInGroup("Player")) return;
        var inventory = body.GetNodeOrNull<InventoryGrid>("Inventory")
                        ?? GetTree().GetFirstNodeInGroup("Inventory") as InventoryGrid;
        if (inventory != null && !inventory.TryAdd(Data))
            return; // inventory full: leave it on the ground
        GD.Print($"Picked up {Data.Name} ({Data.Rarity})");
        EmitSignal(SignalName.PickedUp, body);
        QueueFree();
    }
}
