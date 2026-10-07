// Player.cs (hurtbox mask adjustment)
public override void _Ready()
{
    // Assuming hurtbox is an Area2D child named "Hurtbox"
    var hurtbox = GetNode<Area2D>("Hurtbox");
    // Layer 2 (player), mask 4 (detect only layer 3 – enemy hitboxes)
    hurtbox.CollisionLayer = 1 << 1; // layer 2
    hurtbox.CollisionMask = 1 << 2;  // layer 3
}
