// Player.cs – movement on the brawler "floor" plane, hurtbox layers, health and gear.
//
// Expected children: CollisionShape2D, Area2D "Hurtbox", Health "Health",
// EquipmentComponent "Equipment", InventoryGrid "Inventory" (optional, may live in the UI).
using Godot;

public partial class Player : CharacterBody2D, IDamageable
{
    // Collision layers (1-based in the editor): 2 = player, 3 = enemy hitboxes.
    public const uint PlayerLayer = 1 << 1;
    public const uint EnemyHitboxLayer = 1 << 2;

    [Export] public float Speed { get; set; } = 200f;
    [Export] public float BaseDamage { get; set; } = 10f;
    [Export] public float AttackReach { get; set; } = 48f;
    [Export] public float AttackCooldown { get; set; } = 0.4f;

    public Health Health { get; private set; }
    public EquipmentComponent Equipment { get; private set; }

    private float _lastY = float.MinValue;
    private float _attackTimer;

    public override void _Ready()
    {
        AddToGroup("Player");
        Health = GetNodeOrNull<Health>("Health");
        Equipment = GetNodeOrNull<EquipmentComponent>("Equipment");
        if (Health != null) Health.Died += OnDied;
        if (Equipment != null) Equipment.EquipmentChanged += ApplyGearStats;

        var hurtbox = GetNodeOrNull<Area2D>("Hurtbox");
        if (hurtbox != null)
        {
            hurtbox.CollisionLayer = PlayerLayer;
            hurtbox.CollisionMask = EnemyHitboxLayer; // only enemy hitboxes hurt the player
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Velocity = input * CurrentSpeed; // GetVector is already length-limited to 1
        MoveAndSlide();
        YSort.Update(this, ref _lastY);

        _attackTimer = Mathf.Max(0f, _attackTimer - (float)delta);
        if (Input.IsActionJustPressed("attack") && _attackTimer <= 0f) Attack();
    }

    /// <summary>Hits every enemy within AttackReach of the player.</summary>
    private void Attack()
    {
        _attackTimer = AttackCooldown;
        foreach (var node in GetTree().GetNodesInGroup("Enemy"))
        {
            if (node is Enemy enemy && enemy.GlobalPosition.DistanceTo(GlobalPosition) <= AttackReach)
            {
                enemy.TakeDamage(AttackDamage);
                OnHitLanded();
            }
        }
    }

    /// <summary>Hit damage: (base + added) × (1 + increased%), with a crit roll from gear.</summary>
    public float AttackDamage
    {
        get
        {
            float added = Equipment?.GetStat(StatKey.AddedDamage) ?? 0f;
            float increased = Equipment?.GetStat(StatKey.IncreasedDamage) ?? 0f;
            float crit = 5f + (Equipment?.GetStat(StatKey.CritChance) ?? 0f);
            float damage = (BaseDamage + added) * (1f + increased / 100f);
            return GD.Randf() * 100f < crit ? damage * 1.5f : damage;
        }
    }

    /// <summary>Movement speed after gear (boots, heavy armour penalties).</summary>
    public float CurrentSpeed => Speed * (1f + (Equipment?.GetStat(StatKey.MoveSpeed) ?? 0f) / 100f);

    /// <summary>Call when one of your hits lands: heals from life-on-hit gear.</summary>
    public void OnHitLanded() => Health?.Heal(Equipment?.GetStat(StatKey.LifeOnHit) ?? 0f);

    public void TakeDamage(float amount)
    {
        float armor = Equipment?.GetStat(StatKey.Armor) ?? 0f;
        // Diminishing returns: 100 armor halves damage.
        Health?.ApplyDamage(amount * 100f / (100f + Mathf.Max(0f, armor)));
    }

    private void ApplyGearStats()
    {
        if (Health != null && Equipment != null)
            Health.SetMaxHealth(100f + Equipment.GetStat(StatKey.Health));
    }

    private void OnDied()
    {
        GD.Print("Player died");
        GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene); // placeholder death handling
    }
}
