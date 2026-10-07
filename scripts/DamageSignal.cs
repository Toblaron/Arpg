// DamageSignal.cs
using Godot;

public sealed class DamageSignal : Node
{
    // Typed Godot signal
    [Signal] public delegate void DamageDealtEventHandler(int amount);

    public void Dispatch(int dmg) => EmitSignal(nameof(DamageDealt), dmg);
}
