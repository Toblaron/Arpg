// Enemy.cs – an enemy of a given type (see EnemyType): fights the player in its own style,
// deals damage, drops loot on death.
//
// Expected children: CollisionShape2D, Health "Health", LootDrop "LootDrop" (optional).
using Godot;

public partial class Enemy : CharacterBody2D, IDamageable
{
    [Signal] public delegate void KilledEventHandler(Enemy enemy);

    /// <summary>Which enemy this is: a key of EnemyType.All, e.g. "thug".</summary>
    [Export] public string TypeId { get; set; } = "street_hustler";
    /// <summary>Multiplier on the type's health (waves make enemies tougher).</summary>
    [Export] public float HealthScale { get; set; } = 1f;

    public EnemyType Type { get; private set; }
    public Health Health { get; private set; }

    private const float RangedKeepMin = 110f, RangedKeepMax = 170f;
    private const float SlamEvery = 4.5f, SlamWindup = 0.8f, SlamRadius = 80f, SlamDamageMult = 1.6f;
    private static readonly float[] SummonAt = { 0.6f, 0.3f }; // boss calls backup at these health fractions
    private static readonly string[] Backup = { "thug", "street_hustler" };
    private const float ChargeTrigger = 150f, ChargeWindup = 0.55f, ChargeSpeed = 330f, ChargeTime = 0.4f;

    private Player _target;
    private EnemyLook _look;
    private float _cooldown;
    private float _lastY = float.MinValue;
    private float _stun;
    private Vector2 _knockback;
    private float _time;

    // Charger state: winding up (> 0), then dashing (> 0) along _chargeDir.
    private float _windup, _dash;
    private Vector2 _chargeDir;
    private bool _dashHit;

    // Boss state.
    private float _slamTimer = 2.5f, _slamWindup;
    private int _summons;

    public bool IsStunned => _stun > 0f;
    public bool IsCharging => _dash > 0f;
    public bool IsBoss => Type?.Behaviour == EnemyBehaviour.Boss;
    public bool IsSlamming => _slamWindup > 0f;
    public int BackupCalls => _summons;

    public override void _Ready()
    {
        AddToGroup("Enemy");
        Type = EnemyType.Get(TypeId);
        Health = GetNodeOrNull<Health>("Health");
        if (Health != null)
        {
            Health.Reset(Type.MaxHealth * HealthScale);
            Health.Died += Die;
        }
        if (GetNodeOrNull<LootDrop>("LootDrop") is { } loot)
        {
            loot.MonsterLevel += Type.LootLevelBonus;
            loot.MaxDrops = Type.MaxDrops;
            loot.DropChance = Type.DropChance;
        }

        _look = new EnemyLook { Name = "Look", Type = Type, Seed = (int)(GetInstanceId() % 9973), BaseScale = Type.Size };
        GetNodeOrNull("Body")?.QueueFree(); // the placeholder box
        AddChild(_look);
        _look.SetFacing(-1f);
        if (Type.Size != 1f && GetNodeOrNull<CollisionShape2D>("CollisionShape2D") is { Shape: CapsuleShape2D capsule } col)
            col.Shape = new CapsuleShape2D { Radius = capsule.Radius * Type.Size, Height = capsule.Height * Type.Size };
        if (IsBoss)
        {
            AddToGroup("Boss");
            Health.HealthChanged += OnBossHurt;
        }

        var nameTag = new Label
        {
            Text = Type.DisplayName,
            Position = new Vector2(-50, -38 * Type.Size),
            Size = new Vector2(100, 10),
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            LabelSettings = new LabelSettings { FontSize = 6, FontColor = new Color(1, 1, 1, 0.75f), OutlineSize = 2, OutlineColor = new Color(0, 0, 0, 0.6f) },
        };
        AddChild(nameTag);
        _time = Seed01() * 10f;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        _time += dt;
        _cooldown = Mathf.Max(0f, _cooldown - dt);
        if (_stun > 0f)
        {
            // Knocked back and dazed: slide with the hit, no attacking.
            _stun -= dt;
            _windup = _dash = 0f;
            Velocity = _knockback;
            _knockback = _knockback.MoveToward(Vector2.Zero, 1200f * dt);
            MoveAndSlide();
            GlobalPosition = PlayBounds.ClampDepth(GlobalPosition);
            YSort.Update(this, ref _lastY);
            return;
        }
        _target ??= GetTree().GetFirstNodeInGroup("Player") as Player;
        if (_target == null || !IsInstanceValid(_target)) { Velocity = Vector2.Zero; return; }

        Vector2 toTarget = _target.GlobalPosition - GlobalPosition;
        switch (Type.Behaviour)
        {
            case EnemyBehaviour.Ranged: Ranged(toTarget); break;
            case EnemyBehaviour.Charger: Charger(toTarget, dt); break;
            case EnemyBehaviour.Erratic: ChaseAndHit(toTarget, Wobble(toTarget)); break;
            case EnemyBehaviour.Boss: Boss(toTarget, dt); break;
            default: ChaseAndHit(toTarget, toTarget.Normalized()); break;
        }
        MoveAndSlide();
        GlobalPosition = PlayBounds.ClampDepth(GlobalPosition); // stay on the street
        _look.SetFacing(toTarget.X);
        _look.SetMoving(Velocity.LengthSquared() > 1f);
        YSort.Update(this, ref _lastY);
    }

    private void ChaseAndHit(Vector2 toTarget, Vector2 moveDir)
    {
        if (toTarget.Length() > Type.AttackRange)
        {
            Velocity = moveDir * Type.Speed;
            return;
        }
        Velocity = Vector2.Zero;
        if (_cooldown <= 0f) Hit();
    }

    /// <summary>The Dope Fiend's twitchy approach: zig-zag around the straight line, with jitter.</summary>
    private Vector2 Wobble(Vector2 toTarget)
    {
        Vector2 dir = toTarget.Normalized();
        float sway = Mathf.Sin(_time * 7f) * 0.9f + Mathf.Sin(_time * 23f) * 0.35f;
        return (dir + dir.Orthogonal() * sway).Normalized();
    }

    private void Ranged(Vector2 toTarget)
    {
        float dist = toTarget.Length();
        Vector2 dir = toTarget.Normalized();
        if (dist > RangedKeepMax) Velocity = dir * Type.Speed;
        else if (dist < RangedKeepMin) Velocity = -dir * Type.Speed;
        else Velocity = dir.Orthogonal() * Mathf.Sin(_time * 1.3f) * Type.Speed * 0.5f; // drift sideways

        if (_cooldown <= 0f && dist < RangedKeepMax + 60f)
        {
            _cooldown = Type.AttackCooldown;
            _look.PlayAttack();
            Vector2 from = GlobalPosition + new Vector2(0, -10);
            EnemyProjectile.Throw(this, from, (_target.GlobalPosition - from).Normalized(), Type.Damage);
        }
    }

    private void Charger(Vector2 toTarget, float dt)
    {
        if (_dash > 0f)
        {
            _dash -= dt;
            Velocity = _chargeDir * ChargeSpeed;
            if (!_dashHit && toTarget.Length() < Type.AttackRange + 6f)
            {
                _dashHit = true;
                _target.TakeDamage(Type.Damage);
            }
            if (_dash <= 0f) _cooldown = Type.AttackCooldown;
            return;
        }
        if (_windup > 0f)
        {
            _windup -= dt;
            Velocity = Vector2.Zero;
            if (_windup <= 0f)
            {
                _chargeDir = toTarget.Normalized();
                _dash = ChargeTime;
                _dashHit = false;
                Modulate = Colors.White;
                _look.PlayAttack();
            }
            return;
        }
        if (_cooldown <= 0f && toTarget.Length() < ChargeTrigger)
        {
            _windup = ChargeWindup;
            Modulate = new Color(1.6f, 0.9f, 0.6f); // tell: he's about to charge
            Velocity = Vector2.Zero;
            return;
        }
        ChaseAndHit(toTarget, toTarget.Normalized());
    }

    /// <summary>Pantelåneren: heavy swings up close; every few seconds a telegraphed sledgehammer slam.</summary>
    private void Boss(Vector2 toTarget, float dt)
    {
        if (_slamWindup > 0f)
        {
            _slamWindup -= dt;
            Velocity = Vector2.Zero;
            if (_slamWindup <= 0f) Slam();
            return;
        }
        _slamTimer -= dt;
        if (_slamTimer <= 0f && toTarget.Length() < SlamRadius * 1.6f)
        {
            _slamTimer = SlamEvery;
            _slamWindup = SlamWindup;
            Velocity = Vector2.Zero;
            Modulate = new Color(1.9f, 0.6f, 0.6f); // tell: red glow, hammer raised
            _look.ArmAngle = 1.2f;
            return;
        }
        ChaseAndHit(toTarget, toTarget.Normalized());
    }

    private void Slam()
    {
        Modulate = Colors.White;
        _look.PlayAttack();
        Shockwave.Spawn(this, GlobalPosition + new Vector2(0, 18), SlamRadius);
        if (_target.GlobalPosition.DistanceTo(GlobalPosition) <= SlamRadius)
            _target.TakeDamage(Type.Damage * SlamDamageMult);
    }

    private void OnBossHurt(float current, float max)
    {
        if (_summons >= SummonAt.Length || current <= 0f || current / max > SummonAt[_summons]) return;
        _summons++;
        if (GetTree().GetFirstNodeInGroup("WaveSpawner") is WaveSpawner spawner)
            foreach (string id in Backup) spawner.SpawnAtEdge(id);
    }

    private void Hit()
    {
        _target.TakeDamage(Type.Damage);
        _cooldown = Type.AttackCooldown;
        _look.PlayAttack();
    }

    private float Seed01() => (GetInstanceId() % 1000) / 1000f;

    public void TakeDamage(float amount) => Health?.ApplyDamage(amount);

    /// <summary>Daze for <paramref name="seconds"/> while sliding with <paramref name="knockback"/> (px/s).</summary>
    public void Stun(float seconds, Vector2 knockback)
    {
        float resist = Type?.StunTaken ?? 1f;
        _stun = Mathf.Max(_stun, seconds * resist);
        _knockback = knockback * resist;
        Modulate = new Color(2.5f, 2.5f, 2.5f); // hit flash
        CreateTween().TweenProperty(this, "modulate", Colors.White, 0.25f);
    }

    public void Die()
    {
        EmitSignal(SignalName.Killed, this);
        GetNodeOrNull<LootDrop>("LootDrop")?.Drop(GlobalPosition);
        int kroner = (int)GD.RandRange(Type.KronerMin, Type.KronerMax);
        if (IsBoss)
            for (int i = 0; i < 6; i++) KronerPickup.Drop(this, GlobalPosition, kroner / 6); // a shower of notes
        else
            KronerPickup.Drop(this, GlobalPosition, kroner);
        QueueFree();
    }
}
