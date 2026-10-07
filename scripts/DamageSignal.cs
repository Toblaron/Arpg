// DamageSignal.cs – a relay node for damage numbers, hit sounds, screen shake listeners.
using Godot;

public partial class DamageSignal : Node
{
    [Signal] public delegate void DamageDealtEventHandler(float amount, Vector2 position);

    public void Dispatch(float amount, Vector2 position) => EmitSignal(SignalName.DamageDealt, amount, position);
}
