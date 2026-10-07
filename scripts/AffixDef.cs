// AffixDef.cs – locked definition (now uses an explicit effect type)
using Godot;

public enum StatKey { Damage, AttackSpeed, Health, Armor }
public enum AffixEffectType { Flat, Percent }

[Tool] // visible in the editor
public partial class AffixDef : Resource
{
    [Export] public string Name { get; set; } = "";
    [Export] public StatKey TargetStat { get; set; }
    [Export] public AffixEffectType EffectType { get; set; } = AffixEffectType.Flat;
    [Export] public float Magnitude { get; set; } = 0f; // flat value or percent (e.g. 0.1 for 10%)
    [Export] public float Weight { get; set; } = 1f;
    [Export] public string EffectDescription { get; set; } = "";
}
