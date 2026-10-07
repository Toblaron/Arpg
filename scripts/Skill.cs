// Skill.cs
public enum SkillType
{
    Melee,
    Ranged,
    Magic
}

// ISkill.cs – minimal data a skill needs to cast
public interface ISkill
{
    SkillType Type { get; }
    float Cooldown { get; }
    void Cast(Vector2 from, Vector2 target);
}

// SkillCaster.cs – uses the projectile pool for ranged/magic
public partial class SkillCaster : Node
{
    private readonly ProjectilePool _pool = new();

    public void Cast(ISkill skill, Vector2 from, Vector2 target)
    {
        // Pull stats from equipped weapon if needed
        var equip = GetNode<EquipmentComponent>("/root/Player/Equipment");
        var damage = equip.GetStat("Damage"); // assume stat accessor

        switch (skill.Type)
        {
            case SkillType.Melee:
                // Instant hit – just apply damage
                ApplyDamage(target, damage);
                break;

            case SkillType.Ranged:
            case SkillType.Magic:
                var dir = (target - from).Normalized();
                var proj = _pool.Get(from, dir * skill.GetVelocity());
                proj.Damage = damage;
                proj.OnHit += ApplyDamage;
                break;
        }
    }

    private void ApplyDamage(Vector2 target, float dmg)
    {
        // hit detection placeholder – replace with spatial grid lookup
        var enemies = GetEnemiesAt(target);
        foreach (var e in enemies)
            e.TakeDamage(dmg);
    }
}
