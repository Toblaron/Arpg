// WaveSpawner.cs – sends enemies in waves from the left and right screen edges, along the street.
// The StageDirector starts an encounter (one or more waves) when the screen locks. The wave
// count runs across the whole stage: each wave is bigger, tougher, drops better loot and
// unlocks nastier enemy types.
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class WaveSpawner : Node2D
{
    [Signal] public delegate void WaveStartedEventHandler(int wave, int enemyCount);
    [Signal] public delegate void WaveClearedEventHandler(int wave);
    [Signal] public delegate void EncounterClearedEventHandler();

    [Export] public PackedScene EnemyScene { get; set; }
    [Export] public int FirstWaveSize { get; set; } = 3;
    [Export] public int ExtraPerWave { get; set; } = 1;
    [Export] public int MaxWaveSize { get; set; } = 12;
    [Export] public float SpawnInterval { get; set; } = 0.7f;   // seconds between enemies within a wave
    [Export] public float TimeBetweenWaves { get; set; } = 1.5f; // pause between waves of one encounter
    [Export] public float HealthGrowthPerWave { get; set; } = 0.08f; // +8% enemy health per wave

    /// <summary>The visible screen in world space; enemies enter from just outside its sides. Set by the StageDirector.</summary>
    public Rect2 View { get; set; } = new(0, 0, 1152, 648);

    /// <summary>Which enemies can appear: (type id, first wave it appears, how common).</summary>
    private static readonly (string Id, int FromWave, float Weight)[] Pool =
    {
        ("street_hustler", 1, 3f), ("skank", 1, 3f), ("dope_fiend", 1, 2f),
        ("playa", 2, 2f),
        ("thug", 3, 1.5f), ("drug_dealer", 3, 1.5f),
        ("g", 4, 1.2f),
        ("baller", 5, 0.6f),
    };

    public int CurrentWave { get; private set; }
    /// <summary>Enemies of the current wave still to come plus those alive.</summary>
    public int Remaining => _queue.Count + _alive;
    public bool InEncounter => _running;
    public bool BetweenWaves => _breakTimer > 0f;

    private readonly Queue<string> _queue = new();
    private int _alive;
    private int _wavesLeft;
    private float _spawnTimer;
    private float _breakTimer;
    private bool _running;
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        AddToGroup("WaveSpawner");
        _rng.Randomize();
    }

    public override void _Process(double delta)
    {
        if (!_running) return;
        float dt = (float)delta;
        if (_breakTimer > 0f)
        {
            _breakTimer -= dt;
            if (_breakTimer <= 0f) StartNextWave();
            return;
        }
        _spawnTimer -= dt;
        if (_queue.Count > 0 && _spawnTimer <= 0f)
        {
            _spawnTimer = SpawnInterval;
            Spawn(_queue.Dequeue(), SideEntryPoint());
        }
    }

    /// <summary>Begin an encounter of <paramref name="waves"/> waves, one after the other.</summary>
    public void StartEncounter(int waves)
    {
        _running = true;
        _wavesLeft = Mathf.Max(1, waves);
        StartNextWave();
    }

    private void StartNextWave()
    {
        _wavesLeft--;
        CurrentWave++;
        int count = Mathf.Min(FirstWaveSize + (CurrentWave - 1) * ExtraPerWave, MaxWaveSize);
        foreach (string id in PickTypes(CurrentWave, count)) _queue.Enqueue(id);
        _spawnTimer = 0f;
        EmitSignal(SignalName.WaveStarted, CurrentWave, count);
    }

    /// <summary>The enemy types for a wave: weighted random from what's unlocked, new arrivals guaranteed once.</summary>
    public IEnumerable<string> PickTypes(int wave, int count)
    {
        var unlocked = Pool.Where(p => p.FromWave <= wave).ToArray();
        float total = unlocked.Sum(p => p.Weight);
        var picks = new List<string>();
        // Introduce the types that unlock this wave so the player meets them.
        picks.AddRange(unlocked.Where(p => p.FromWave == wave && wave > 1).Select(p => p.Id).Take(count));
        while (picks.Count < count)
        {
            float roll = _rng.Randf() * total;
            string pick = unlocked[^1].Id; // float rounding fallback
            foreach (var p in unlocked)
            {
                roll -= p.Weight;
                if (roll <= 0f) { pick = p.Id; break; }
            }
            picks.Add(pick);
        }
        return picks.OrderBy(_ => _rng.Randi());
    }

    /// <summary>Spawn one enemy of <paramref name="typeId"/> at <paramref name="at"/>, scaled for the current wave.</summary>
    public Enemy Spawn(string typeId, Vector2 at)
    {
        var enemy = (EnemyScene ?? GD.Load<PackedScene>("res://scenes/Enemy.tscn")).Instantiate<Enemy>();
        int wave = Mathf.Max(CurrentWave, 1);
        enemy.TypeId = typeId;
        enemy.HealthScale = 1f + HealthGrowthPerWave * (wave - 1);
        if (enemy.GetNodeOrNull<LootDrop>("LootDrop") is { } loot) loot.MonsterLevel = 1 + (wave - 1) / 2;
        enemy.Position = PlayBounds.ClampDepth(at);
        _alive++;
        enemy.Killed += OnEnemyKilled;
        (GetTree().CurrentScene ?? GetParent()).AddChild(enemy);
        return enemy;
    }

    private void OnEnemyKilled(Enemy enemy)
    {
        _alive--;
        if (!_running || _alive > 0 || _queue.Count > 0 || _breakTimer > 0f) return;
        EmitSignal(SignalName.WaveCleared, CurrentWave);
        if (_wavesLeft > 0)
        {
            _breakTimer = TimeBetweenWaves;
            return;
        }
        _running = false;
        EmitSignal(SignalName.EncounterCleared);
    }

    /// <summary>Spawn one enemy just off a screen edge (the boss's backup).</summary>
    public Enemy SpawnAtEdge(string typeId) => Spawn(typeId, SideEntryPoint());

    /// <summary>The stage boss, walking in from the right; tougher each stage.</summary>
    public Enemy SpawnBoss(int stage)
    {
        var boss = (EnemyScene ?? GD.Load<PackedScene>("res://scenes/Enemy.tscn")).Instantiate<Enemy>();
        boss.TypeId = EnemyType.BossId;
        boss.HealthScale = 1f + 0.5f * (stage - 1);
        if (boss.GetNodeOrNull<LootDrop>("LootDrop") is { } loot) loot.MonsterLevel = 2 + stage * 2;
        boss.Position = PlayBounds.ClampDepth(new Vector2(View.End.X + 50f, (PlayBounds.Top + PlayBounds.Bottom) / 2f));
        (GetTree().CurrentScene ?? GetParent()).AddChild(boss);
        return boss;
    }

    /// <summary>Remove every enemy and stop any encounter (stage over or restarting).</summary>
    public void ClearAll()
    {
        _queue.Clear();
        _alive = 0;
        _running = false;
        _breakTimer = 0f;
        foreach (var n in GetTree().GetNodesInGroup("Enemy")) n.QueueFree();
        foreach (var n in GetTree().GetNodesInGroup("EnemyProjectile")) n.QueueFree();
    }

    /// <summary>Just off the left or right edge of the screen, somewhere on the street.</summary>
    private Vector2 SideEntryPoint()
    {
        const float outside = 40f;
        float x = _rng.Randf() < 0.5f ? View.Position.X - outside : View.End.X + outside;
        float top = Mathf.Max(PlayBounds.Top, View.Position.Y + 60f);
        float bottom = Mathf.Min(PlayBounds.Bottom, View.End.Y - 30f);
        return new Vector2(x, _rng.RandfRange(top, Mathf.Max(top, bottom)));
    }
}
