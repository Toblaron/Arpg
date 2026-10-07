// IDamageable.cs – anything that can be hit: players, enemies, breakables.
public interface IDamageable
{
    void TakeDamage(float amount);
}
