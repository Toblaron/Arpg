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

    [Export] public float Speed { get; set; } = 160f;
    [Export] public float BaseDamage { get; set; } = 10f;
    [Export] public float AttackReach { get; set; } = 48f;
    [Export] public float AttackCooldown { get; set; } = 0.4f;
    /// <summary>Which playable class this is (see PlayerClass.Create), e.g. "pimp".</summary>
    [Export] public string ClassId { get; set; } = PimpClass.ClassId;

    /// <summary>Raised with the new balance when kroner are picked up or spent.</summary>
    [Signal] public delegate void KronerChangedEventHandler(int kroner);

    public Health Health { get; private set; }
    public EquipmentComponent Equipment { get; private set; }
    /// <summary>Norwegian kroner: dropped by enemies, spent at Matkroken.</summary>
    public int Kroner { get; private set; }
    /// <summary>Permanent upgrades bought at Matkroken.</summary>
    public PlayerUpgrades Upgrades { get; } = new();
    public PlayerClass Class { get; private set; }
    /// <summary>Left or right; the class ability and effects aim this way.</summary>
    public Vector2 Facing { get; private set; } = Vector2.Right;
    public float AbilityCooldownRemaining => _abilityTimer;

    private float _lastY = float.MinValue;
    private float _attackTimer;
    private float _abilityTimer;
    private ClassLook _look;

    public override void _Ready()
    {
        AddToGroup("Player");
        Class = PlayerClass.Create(ClassId);
        _look = Class.CreateLook();
        if (_look != null)
        {
            GetNodeOrNull("Body")?.QueueFree(); // the placeholder box
            AddChild(_look);
        }
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
        GlobalPosition = PlayBounds.ClampPlayer(GlobalPosition); // stay on the street and on screen
        YSort.Update(this, ref _lastY);
        if (input.X != 0f) Facing = new Vector2(Mathf.Sign(input.X), 0f);
        _look?.SetFacing(Facing.X);
        _look?.SetMoving(input != Vector2.Zero);

        _attackTimer = Mathf.Max(0f, _attackTimer - (float)delta);
        _abilityTimer = Mathf.Max(0f, _abilityTimer - (float)delta);
        if (Input.IsActionJustPressed("attack") && _attackTimer <= 0f) Attack();
        if (Input.IsActionJustPressed("ability") && _abilityTimer <= 0f) UseAbility();
    }

    /// <summary>The class's unique ability (the Pimp's Bitch-Slap).</summary>
    private void UseAbility()
    {
        _abilityTimer = Class.AbilityCooldown * Upgrades.AbilityCooldownMultiplier;
        _look?.PlayAbility();
        Class.UseAbility(this);
    }

    /// <summary>Hits every enemy within AttackReach of the player.</summary>
    private void Attack()
    {
        _attackTimer = AttackCooldown;
        _look?.PlayAttack();
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
            float damage = (BaseDamage + added) * (1f + increased / 100f) * Upgrades.DamageMultiplier;
            return GD.Randf() * 100f < crit ? damage * 1.5f : damage;
        }
    }

    /// <summary>Movement speed after gear (boots, heavy armour penalties).</summary>
    public float CurrentSpeed => Speed * (1f + (Equipment?.GetStat(StatKey.MoveSpeed) ?? 0f) / 100f) * Upgrades.SpeedMultiplier;

    /// <summary>Magic find from gear plus Matkroken scratch cards.</summary>
    public float MagicFind => (Equipment?.GetStat(StatKey.MagicFind) ?? 0f) + Upgrades.MagicFindBonus;

    public void AddKroner(int amount)
    {
        if (amount <= 0) return;
        Kroner += amount;
        EmitSignal(SignalName.KronerChanged, Kroner);
    }

    /// <summary>Pay <paramref name="amount"/> kroner if there's enough. False (and nothing spent) otherwise.</summary>
    public bool TrySpend(int amount)
    {
        if (amount < 0 || amount > Kroner) return false;
        Kroner -= amount;
        EmitSignal(SignalName.KronerChanged, Kroner);
        return true;
    }

    /// <summary>How many times a Matkroken upgrade was bought (also callable from GDScript).</summary>
    public int UpgradeLevel(string id) => Upgrades.Level(id);

    /// <summary>Re-apply everything that sets max health (gear and upgrades).</summary>
    public void RefreshStats() => ApplyGearStats();

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
        if (Health != null)
            Health.SetMaxHealth(100f + (Equipment?.GetStat(StatKey.Health) ?? 0f) + Upgrades.MaxHealthBonus);
    }

    private void OnDied()
    {
        GD.Print("Player died");
        GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene); // placeholder death handling
    }
}
