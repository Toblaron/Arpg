// WaveSpawner.cs – sends enemies in waves from the screen edges. Each wave is bigger, tougher
// and drops better loot, and unlocks nastier enemy types. Next wave starts a few seconds after
// the last enemy of the current one dies.
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class WaveSpawner : Node2D
{
    [Signal] public delegate void WaveStartedEventHandler(int wave, int enemyCount);
    [Signal] public delegate void WaveClearedEventHandler(int wave);

    [Export] public PackedScene EnemyScene { get; set; }
    [Export] public bool AutoStart { get; set; } = true;
    [Export] public int FirstWaveSize { get; set; } = 3;
    [Export] public int ExtraPerWave { get; set; } = 1;
    [Export] public int MaxWaveSize { get; set; } = 14;
    [Export] public float SpawnInterval { get; set; } = 0.7f;   // seconds between enemies within a wave
    [Export] public float TimeBetweenWaves { get; set; } = 3f;
    [Export] public float HealthGrowthPerWave { get; set; } = 0.08f; // +8% enemy health per wave
    /// <summary>The play area; enemies enter from just outside it.</summary>
    [Export] public Rect2 Arena { get; set; } = new(0, 0, 1152, 648);

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
    /// <summary>Enemies of this wave still to come plus those alive.</summary>
    public int Remaining => _queue.Count + _alive;
    public bool BetweenWaves => _breakTimer > 0f;
    public float TimeToNextWave => _breakTimer;

    private readonly Queue<string> _queue = new();
    private int _alive;
    private float _spawnTimer;
    private float _breakTimer;
    private bool _running;
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        _rng.Randomize();
        // Deferred so the UI (later in the tree) is listening when wave 1 is announced.
        if (AutoStart) CallDeferred(MethodName.StartNextWave);
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
            Spawn(_queue.Dequeue(), RandomEdgePoint());
        }
    }

    public void StartNextWave()
    {
        _running = true;
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
            foreach (var p in unlocked)
            {
                roll -= p.Weight;
                if (roll <= 0f) { picks.Add(p.Id); break; }
            }
            if (roll > 0f) picks.Add(unlocked[^1].Id); // float rounding
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
        enemy.Position = at;
        _alive++;
        enemy.Killed += OnEnemyKilled;
        (GetTree().CurrentScene ?? GetParent()).AddChild(enemy);
        return enemy;
    }

    private void OnEnemyKilled(Enemy enemy)
    {
        _alive--;
        if (_running && _alive <= 0 && _queue.Count == 0 && _breakTimer <= 0f)
        {
            EmitSignal(SignalName.WaveCleared, CurrentWave);
            _breakTimer = TimeBetweenWaves;
        }
    }

    private Vector2 RandomEdgePoint()
    {
        const float outside = 30f;
        Rect2 a = Arena;
        return _rng.RandiRange(0, 3) switch
        {
            0 => new Vector2(a.Position.X - outside, _rng.RandfRange(a.Position.Y + 60, a.End.Y - 160)),
            1 => new Vector2(a.End.X + outside, _rng.RandfRange(a.Position.Y + 60, a.End.Y - 160)),
            2 => new Vector2(_rng.RandfRange(a.Position.X + 60, a.End.X - 60), a.Position.Y - outside),
            _ => new Vector2(_rng.RandfRange(a.Position.X + 720, a.End.X - 60), a.End.Y + outside), // bottom right, clear of the inventory
        };
    }
}
