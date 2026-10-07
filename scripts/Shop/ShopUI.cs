// ShopUI.cs – Matkroken, the corner shop between stages. Opens after the boss is beaten, pauses
// the game, sells the upgrades in ShopCatalog for kroner, and sends the player on to the next stage.
using System.Collections.Generic;
using Godot;

public partial class ShopUI : Control
{
    [Export] public Player Player { get; set; }
    [Export] public StageDirector Director { get; set; }
    /// <summary>Seconds after the boss falls before the shop opens (lets "STAGE CLEAR" play).</summary>
    [Export] public float OpenDelay { get; set; } = 3f;

    private static readonly Color Red = new(0.78f, 0.1f, 0.12f), Cream = new(0.98f, 0.95f, 0.86f);
    private static readonly Color Ink = new(0.12f, 0.1f, 0.1f), Muted = new(0.4f, 0.36f, 0.33f);

    public bool IsOpen => Visible;

    private Label _subtitle, _wallet, _health;
    private Button _continue;
    private readonly Dictionary<string, (Label Level, Label Price, Button Buy)> _rows = new();

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; // works while the game is paused
        SetAnchorsPreset(LayoutPreset.FullRect);
        Build();
        Hide();
        if (Director != null) Director.StageCleared += OnStageCleared;
        if (Player != null) Player.KronerChanged += _ => Refresh();
    }

    private async void OnStageCleared()
    {
        await ToSignal(GetTree().CreateTimer(OpenDelay), SceneTreeTimer.SignalName.Timeout);
        if (Director?.Cleared == true) Open();
    }

    public void Open()
    {
        _subtitle.Text = $"Stage {Director?.Stage ?? 1} cleared. Stock up before the next round.";
        _continue.Text = $"On to stage {(Director?.Stage ?? 1) + 1}  →";
        Refresh();
        Show();
        GetTree().Paused = true;
        _continue.GrabFocus();
    }

    /// <summary>Buy one of <paramref name="id"/> if affordable. Returns whether it was bought.</summary>
    public bool Buy(string id)
    {
        var item = ShopCatalog.Get(id);
        if (item == null || Player == null) return false;
        int level = Player.Upgrades.Level(id);
        if (!item.Consumable && level >= item.MaxLevel) return false;
        if (!Player.TrySpend(item.PriceAt(item.Consumable ? 0 : level))) return false;
        if (item.Consumable)
        {
            if (id == "polse") Player.Health?.Heal(Player.Health.MaxHealth);
        }
        else
        {
            Player.Upgrades.Add(id);
            Player.RefreshStats();
        }
        Refresh();
        return true;
    }

    public void Continue()
    {
        Hide();
        GetTree().Paused = false;
        Director?.StartStage(Director.Stage + 1);
    }

    private void Refresh()
    {
        if (Player == null) return;
        _wallet.Text = $"You have {Kr(Player.Kroner)}";
        _health.Text = Player.Health != null ? $"Health {Player.Health.Current:0} / {Player.Health.MaxHealth:0}" : "";
        foreach (var item in ShopCatalog.Items)
        {
            var (levelLabel, priceLabel, buy) = _rows[item.Id];
            int level = Player.Upgrades.Level(item.Id);
            bool maxed = !item.Consumable && level >= item.MaxLevel;
            int price = item.PriceAt(item.Consumable ? 0 : level);
            levelLabel.Text = item.Consumable ? "" : $"Lv {level}/{item.MaxLevel}";
            priceLabel.Text = maxed ? "SOLD OUT" : Kr(price);
            bool full = item.Id == "polse" && Player.Health != null && Player.Health.Current >= Player.Health.MaxHealth;
            buy.Disabled = maxed || full || price > Player.Kroner;
            buy.Text = full ? "Full" : "Buy";
        }
    }

    /// <summary>Norwegian style: "1 250 kr".</summary>
    public static string Kr(int amount) => $"{amount:#,0} kr".Replace(",", " ").Replace(" ", " ");

    // ---------- layout ----------

    private void Build()
    {
        AddChild(new ColorRect { Color = new Color(0, 0, 0, 0.6f), MouseFilter = MouseFilterEnum.Stop, AnchorRight = 1, AnchorBottom = 1 });

        var panel = new PanelContainer { Position = new Vector2(196, 64), Size = new Vector2(760, 520) };
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = Cream, CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10, CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10,
            BorderColor = Red, BorderWidthTop = 4, BorderWidthBottom = 4, BorderWidthLeft = 4, BorderWidthRight = 4,
            ContentMarginLeft = 0, ContentMarginRight = 0, ContentMarginTop = 0, ContentMarginBottom = 18,
        });
        AddChild(panel);
        var col = new VBoxContainer();
        col.AddThemeConstantOverride("separation", 10);
        panel.AddChild(col);

        // Shop sign.
        var sign = new PanelContainer();
        sign.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = Red, CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6, ContentMarginTop = 10, ContentMarginBottom = 10 });
        col.AddChild(sign);
        sign.AddChild(MakeLabel("MATKROKEN", 40, Cream, HorizontalAlignment.Center, outline: true));

        _subtitle = MakeLabel("", 16, Muted, HorizontalAlignment.Center);
        col.AddChild(_subtitle);

        var info = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        info.AddThemeConstantOverride("separation", 40);
        _wallet = MakeLabel("", 20, Ink);
        _health = MakeLabel("", 20, Ink);
        info.AddChild(_wallet);
        info.AddChild(_health);
        col.AddChild(info);

        var grid = new GridContainer { Columns = 3, SizeFlagsHorizontal = SizeFlags.ShrinkCenter };
        grid.AddThemeConstantOverride("h_separation", 12);
        grid.AddThemeConstantOverride("v_separation", 12);
        col.AddChild(grid);
        foreach (var item in ShopCatalog.Items) grid.AddChild(MakeCard(item));

        _continue = new Button { CustomMinimumSize = new Vector2(320, 46), SizeFlagsHorizontal = SizeFlags.ShrinkCenter };
        _continue.AddThemeFontSizeOverride("font_size", 20);
        StyleButton(_continue, Ink);
        _continue.Pressed += Continue;
        col.AddChild(_continue);
    }

    private Control MakeCard(ShopItem item)
    {
        var card = new PanelContainer { CustomMinimumSize = new Vector2(232, 132) };
        card.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = Colors.White, BorderColor = new Color(0.85f, 0.8f, 0.7f), BorderWidthTop = 2, BorderWidthBottom = 2, BorderWidthLeft = 2, BorderWidthRight = 2,
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6, CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
            ContentMarginLeft = 12, ContentMarginRight = 12, ContentMarginTop = 8, ContentMarginBottom = 8,
        });
        var box = new VBoxContainer();
        box.AddThemeConstantOverride("separation", 2);
        card.AddChild(box);

        var top = new HBoxContainer();
        var name = MakeLabel(item.Name, 18, Ink);
        name.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var level = MakeLabel("", 13, Muted);
        top.AddChild(name);
        top.AddChild(level);
        box.AddChild(top);

        string desc = item.Id == "kaffe" && Player?.Class != null ? $"{Player.Class.AbilityName} cooldown −15%" : item.Description;
        var descLabel = MakeLabel(desc, 14, Muted);
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        descLabel.CustomMinimumSize = new Vector2(200, 40);
        box.AddChild(descLabel);

        var bottom = new HBoxContainer();
        var price = MakeLabel("", 18, Red);
        price.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var buy = new Button { Text = "Buy", CustomMinimumSize = new Vector2(70, 30) };
        StyleButton(buy, Red);
        buy.Pressed += () => Buy(item.Id);
        bottom.AddChild(price);
        bottom.AddChild(buy);
        box.AddChild(bottom);

        _rows[item.Id] = (level, price, buy);
        return card;
    }

    private static void StyleButton(Button button, Color color)
    {
        StyleBoxFlat Box(Color c) => new()
        {
            BgColor = c, CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5,
            ContentMarginLeft = 10, ContentMarginRight = 10, ContentMarginTop = 4, ContentMarginBottom = 4,
        };
        button.AddThemeStyleboxOverride("normal", Box(color));
        button.AddThemeStyleboxOverride("hover", Box(color.Lightened(0.15f)));
        button.AddThemeStyleboxOverride("pressed", Box(color.Darkened(0.2f)));
        var focus = Box(color.Lightened(0.15f));
        focus.BorderColor = new Color(1f, 0.8f, 0.18f);
        focus.BorderWidthTop = focus.BorderWidthBottom = focus.BorderWidthLeft = focus.BorderWidthRight = 3;
        button.AddThemeStyleboxOverride("focus", focus);
        button.AddThemeStyleboxOverride("disabled", Box(new Color(0.82f, 0.8f, 0.76f)));
        button.AddThemeColorOverride("font_color", Colors.White);
        button.AddThemeColorOverride("font_hover_color", Colors.White);
        button.AddThemeColorOverride("font_focus_color", Colors.White);
        button.AddThemeColorOverride("font_disabled_color", new Color(0.55f, 0.52f, 0.48f));
    }

    private static Label MakeLabel(string text, int size, Color color, HorizontalAlignment align = HorizontalAlignment.Left, bool outline = false) => new()
    {
        Text = text,
        HorizontalAlignment = align,
        LabelSettings = new LabelSettings { FontSize = size, FontColor = color, OutlineSize = outline ? 6 : 0, OutlineColor = new Color(0.4f, 0.03f, 0.05f) },
    };
}
