// PimpClass.cs – the Pimp. Unique ability: Bitch-Slap, a huge backhand to the front that
// hits every enemy in reach for triple damage, knocks them back and stuns them.
using Godot;

public sealed class PimpClass : PlayerClass
{
    public const string ClassId = "pimp";

    public const float SlapReach = 72f;
    public const float SlapDamageMultiplier = 3f;
    public const float SlapKnockback = 420f;   // px/s at the moment of impact
    public const float SlapStun = 1.2f;        // seconds
    private const float FrontArc = 0.2f;       // dot product: >0.2 ≈ within 78° of facing

    public override string Id => ClassId;
    public override string DisplayName => "Pimp";
    public override string AbilityName => "Bitch-Slap";
    public override float AbilityCooldown => 4f;

    public override ClassLook CreateLook() => new PimpLook { Name = "Look" };

    public override void UseAbility(Player player)
    {
        Vector2 origin = player.GlobalPosition;
        int hits = 0;
        foreach (var node in player.GetTree().GetNodesInGroup("Enemy"))
        {
            if (node is not Enemy enemy || enemy.IsQueuedForDeletion()) continue;
            Vector2 to = enemy.GlobalPosition - origin;
            if (to.Length() > SlapReach) continue;
            Vector2 dir = to.LengthSquared() > 1f ? to.Normalized() : player.Facing;
            if (dir.Dot(player.Facing) < FrontArc) continue;

            enemy.Stun(SlapStun, dir * SlapKnockback);
            enemy.TakeDamage(player.AttackDamage * SlapDamageMultiplier);
            player.OnHitLanded();
            hits++;
        }
        SlapEffect.Spawn(player, hits > 0);
    }
}
