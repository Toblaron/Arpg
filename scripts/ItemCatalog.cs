// ItemCatalog.cs – item bases and affixes, D2/PoE style.
//
// An item is a BASE (Short Sword, Chain Mail…) that sets its slot and implicit stats, plus
// random AFFIXES: prefixes and suffixes, each from a family (one per family per item) and a
// TIER gated by item level. Higher item levels unlock better bases and stronger tiers.
// Add bases and affixes by editing the tables below.
using System.Collections.Generic;
using System.Linq;

public enum ItemSlot { Weapon, Head, Chest, Gloves, Boots, Belt, Amulet, Ring }

public enum Rarity { Normal, Magic, Rare, Unique }

public enum AffixKind { Prefix, Suffix }

/// <summary>A stat range rolled once when the item drops: Min..Max inclusive.</summary>
public readonly record struct StatRange(StatKey Stat, float Min, float Max);

public sealed record ItemBase(
    string Id,
    string Name,
    ItemSlot Slot,
    int DropLevel,          // never drops from monsters below this level
    float Weight,           // relative drop frequency
    StatRange[] Implicits); // rolled base stats (weapon damage, armor, …)

public sealed record AffixTier(int Tier, int MinItemLevel, string Name, float Min, float Max, float Weight);

/// <summary>One affix family, e.g. "+# to maximum Life", with tiers from weakest to strongest.</summary>
public sealed record AffixDef(
    string Id,
    AffixKind Kind,
    StatKey Stat,
    ItemSlot[] Slots,
    AffixTier[] Tiers);

public static class ItemCatalog
{
    static readonly ItemSlot[] Weapon = { ItemSlot.Weapon };
    static readonly ItemSlot[] Armour = { ItemSlot.Head, ItemSlot.Chest, ItemSlot.Gloves, ItemSlot.Boots, ItemSlot.Belt };
    static readonly ItemSlot[] Jewelry = { ItemSlot.Amulet, ItemSlot.Ring };
    static ItemSlot[] Of(params ItemSlot[][] groups) => groups.SelectMany(g => g).Distinct().ToArray();
    static StatRange Dmg(float min, float max) => new(StatKey.AddedDamage, min, max);
    static StatRange Arm(float min, float max) => new(StatKey.Armor, min, max);

    public static readonly ItemBase[] Bases =
    {
        // Weapons: implicit damage
        new("short_sword",  "Short Sword",   ItemSlot.Weapon, 1,  100, new[] { Dmg(4, 9) }),
        new("hand_axe",     "Hand Axe",      ItemSlot.Weapon, 1,  100, new[] { Dmg(3, 11) }),
        new("war_hammer",   "War Hammer",    ItemSlot.Weapon, 8,  80,  new[] { Dmg(8, 18), new StatRange(StatKey.AttackSpeed, -10, -10) }),
        new("broad_sword",  "Broad Sword",   ItemSlot.Weapon, 12, 70,  new[] { Dmg(10, 22) }),
        new("grim_scythe",  "Grim Scythe",   ItemSlot.Weapon, 25, 50,  new[] { Dmg(18, 36), new StatRange(StatKey.CritChance, 3, 3) }),
        // Armour: implicit armor (and a little extra on some)
        new("leather_cap",  "Leather Cap",   ItemSlot.Head,   1,  100, new[] { Arm(3, 6) }),
        new("great_helm",   "Great Helm",    ItemSlot.Head,   15, 70,  new[] { Arm(14, 24) }),
        new("quilted_vest", "Quilted Vest",  ItemSlot.Chest,  1,  100, new[] { Arm(8, 14) }),
        new("chain_mail",   "Chain Mail",    ItemSlot.Chest,  10, 80,  new[] { Arm(22, 34) }),
        new("plate_armor",  "Plate Armor",   ItemSlot.Chest,  22, 50,  new[] { Arm(45, 65), new StatRange(StatKey.MoveSpeed, -3, -3) }),
        new("leather_gloves","Leather Gloves",ItemSlot.Gloves, 1,  100, new[] { Arm(2, 4) }),
        new("gauntlets",    "Gauntlets",     ItemSlot.Gloves, 14, 70,  new[] { Arm(10, 16) }),
        new("light_boots",  "Light Boots",   ItemSlot.Boots,  1,  100, new[] { Arm(2, 4), new StatRange(StatKey.MoveSpeed, 5, 5) }),
        new("greaves",      "Greaves",       ItemSlot.Boots,  14, 70,  new[] { Arm(10, 16) }),
        new("sash",         "Sash",          ItemSlot.Belt,   1,  100, new[] { new StatRange(StatKey.Health, 5, 10) }),
        new("heavy_belt",   "Heavy Belt",    ItemSlot.Belt,   12, 70,  new[] { new StatRange(StatKey.Health, 15, 25) }),
        // Jewelry: no implicit, or a small one
        new("ring",         "Ring",          ItemSlot.Ring,   1,  60,  new StatRange[0]),
        new("coral_ring",   "Coral Ring",    ItemSlot.Ring,   5,  40,  new[] { new StatRange(StatKey.Health, 10, 20) }),
        new("amulet",       "Amulet",        ItemSlot.Amulet, 1,  60,  new StatRange[0]),
        new("jade_amulet",  "Jade Amulet",   ItemSlot.Amulet, 10, 40,  new[] { new StatRange(StatKey.MagicFind, 5, 10) }),
    };

    public static readonly AffixDef[] Affixes =
    {
        // ---- Prefixes ----
        new("added_damage", AffixKind.Prefix, StatKey.AddedDamage, Of(Weapon, Jewelry, new[] { ItemSlot.Gloves }), new AffixTier[]
        {
            new(1, 1,  "Glinting",  1, 3, 1000), new(2, 8,  "Burnished", 3, 6, 800),
            new(3, 16, "Polished",  6, 10, 600), new(4, 28, "Honed",     10, 16, 400),
        }),
        new("increased_damage", AffixKind.Prefix, StatKey.IncreasedDamage, Weapon, new AffixTier[]
        {
            new(1, 1,  "Heavy",    20, 34, 1000), new(2, 10, "Serrated", 35, 49, 700),
            new(3, 20, "Vicious",  50, 69, 500),  new(4, 32, "Merciless", 70, 89, 250),
        }),
        new("life", AffixKind.Prefix, StatKey.Health, Of(Armour, Jewelry), new AffixTier[]
        {
            new(1, 1,  "Healthy",  10, 19, 1000), new(2, 9,  "Sanguine", 20, 34, 800),
            new(3, 18, "Stalwart", 35, 49, 600),  new(4, 30, "Vigorous", 50, 69, 350),
        }),
        new("flat_armor", AffixKind.Prefix, StatKey.Armor, Armour, new AffixTier[]
        {
            new(1, 1,  "Lacquered", 5, 12, 1000), new(2, 10, "Studded",  13, 24, 700),
            new(3, 22, "Ribbed",    25, 40, 450),
        }),
        new("increased_armor", AffixKind.Prefix, StatKey.IncreasedArmor, Armour, new AffixTier[]
        {
            new(1, 1,  "Reinforced", 15, 26, 1000), new(2, 12, "Layered",  27, 42, 700),
            new(3, 24, "Fortified",  43, 60, 400),
        }),
        // ---- Suffixes ----
        new("attack_speed", AffixKind.Suffix, StatKey.AttackSpeed, Of(Weapon, new[] { ItemSlot.Gloves, ItemSlot.Ring }), new AffixTier[]
        {
            new(1, 1,  "of Skill",  5, 7, 1000), new(2, 11, "of Ease", 8, 10, 700),
            new(3, 22, "of Mastery", 11, 13, 450), new(4, 34, "of Grandmastery", 14, 16, 200),
        }),
        new("crit", AffixKind.Suffix, StatKey.CritChance, Of(Weapon, new[] { ItemSlot.Amulet }), new AffixTier[]
        {
            new(1, 1, "of Needling", 2, 4, 1000), new(2, 14, "of Stinging", 5, 7, 600), new(3, 28, "of Piercing", 8, 10, 300),
        }),
        new("life_on_hit", AffixKind.Suffix, StatKey.LifeOnHit, Of(Weapon, new[] { ItemSlot.Gloves, ItemSlot.Ring }), new AffixTier[]
        {
            new(1, 1, "of the Leech", 1, 2, 1000), new(2, 15, "of the Lamprey", 3, 5, 600), new(3, 30, "of the Vampire", 6, 9, 300),
        }),
        new("fire_res", AffixKind.Suffix, StatKey.FireResistance, Of(Armour, Jewelry), new AffixTier[]
        {
            new(1, 1, "of the Whelpling", 6, 11, 1000), new(2, 12, "of the Salamander", 12, 23, 700), new(3, 24, "of the Drake", 24, 35, 450),
        }),
        new("cold_res", AffixKind.Suffix, StatKey.ColdResistance, Of(Armour, Jewelry), new AffixTier[]
        {
            new(1, 1, "of the Inuit", 6, 11, 1000), new(2, 12, "of the Seal", 12, 23, 700), new(3, 24, "of the Penguin", 24, 35, 450),
        }),
        new("lightning_res", AffixKind.Suffix, StatKey.LightningResistance, Of(Armour, Jewelry), new AffixTier[]
        {
            new(1, 1, "of the Cloud", 6, 11, 1000), new(2, 12, "of the Squall", 12, 23, 700), new(3, 24, "of the Storm", 24, 35, 450),
        }),
        new("move_speed", AffixKind.Suffix, StatKey.MoveSpeed, new[] { ItemSlot.Boots }, new AffixTier[]
        {
            new(1, 1, "of the Hare", 5, 9, 1000), new(2, 12, "of the Gazelle", 10, 14, 600), new(3, 26, "of the Cheetah", 15, 20, 300),
        }),
        new("magic_find", AffixKind.Suffix, StatKey.MagicFind, Of(Jewelry, new[] { ItemSlot.Head, ItemSlot.Boots }), new AffixTier[]
        {
            new(1, 1, "of Plunder", 6, 10, 800), new(2, 14, "of Fortune", 11, 18, 500), new(3, 30, "of Greed", 19, 26, 250),
        }),
    };

    static readonly Dictionary<string, ItemBase> _basesById = Bases.ToDictionary(b => b.Id);

    public static ItemBase GetBase(string id) => _basesById.TryGetValue(id, out var b) ? b : null;

    // Rare names are two random words, D2 style: "Doom Grip", "Storm Song".
    public static readonly string[] RareFirst =
    {
        "Agony", "Apocalypse", "Armageddon", "Beast", "Behemoth", "Blight", "Blood", "Bramble", "Brimstone",
        "Brood", "Carrion", "Cataclysm", "Corpse", "Corruption", "Damnation", "Death", "Demon", "Dire", "Dragon",
        "Dread", "Doom", "Dusk", "Eagle", "Empyrean", "Fate", "Foe", "Gale", "Ghoul", "Gloom", "Glyph", "Golem",
        "Grim", "Hate", "Havoc", "Honour", "Horror", "Hypnotic", "Kraken", "Loath", "Maelstrom", "Mind", "Miracle",
        "Morbid", "Oblivion", "Onslaught", "Pain", "Pandemonium", "Phoenix", "Plague", "Rage", "Rapture", "Rune",
        "Skull", "Sol", "Soul", "Sorrow", "Spirit", "Storm", "Tempest", "Torment", "Vengeance", "Victory", "Viper",
        "Vortex", "Woe", "Wrath",
    };

    public static readonly Dictionary<ItemSlot, string[]> RareSecond = new()
    {
        [ItemSlot.Weapon] = new[] { "Bane", "Bite", "Edge", "Fang", "Gnash", "Hunger", "Kiss", "Mangler", "Razor", "Reaver", "Scalpel", "Song", "Spike", "Sunder", "Thirst" },
        [ItemSlot.Head] = new[] { "Brow", "Corona", "Crest", "Crown", "Dome", "Halo", "Horn", "Keep", "Peak", "Visage", "Visor" },
        [ItemSlot.Chest] = new[] { "Carapace", "Cloak", "Coat", "Hide", "Jack", "Keep", "Mantle", "Pelt", "Shell", "Suit", "Wrap" },
        [ItemSlot.Gloves] = new[] { "Caress", "Claw", "Clutches", "Fingers", "Fist", "Grasp", "Grip", "Hand", "Hold", "Knuckle", "Talons" },
        [ItemSlot.Boots] = new[] { "Dash", "Goad", "Hoof", "League", "March", "Pace", "Road", "Slippers", "Sole", "Span", "Spur", "Stride", "Track", "Trail", "Tread" },
        [ItemSlot.Belt] = new[] { "Bind", "Bond", "Buckle", "Chain", "Cord", "Coil", "Fetter", "Girdle", "Lash", "Lock", "Strap", "Tether" },
        [ItemSlot.Amulet] = new[] { "Beads", "Charm", "Clasp", "Collar", "Heart", "Idol", "Locket", "Medal", "Noose", "Pendant", "Scarab", "Talisman" },
        [ItemSlot.Ring] = new[] { "Band", "Circle", "Coil", "Eye", "Finger", "Grasp", "Hold", "Knot", "Loop", "Nail", "Spiral", "Twirl", "Whorl" },
    };
}
