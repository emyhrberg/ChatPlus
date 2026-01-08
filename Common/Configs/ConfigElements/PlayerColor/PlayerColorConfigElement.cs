using System;
using System.Globalization;
using ChatPlus.Common.Configs.ConfigElements.PlayerColor;
using ChatPlus.Core.Features.PlayerColors;
using ChatPlus.Core.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.OS;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;
using static Terraria.GameContent.UI.States.UICharacterCreation;

namespace ChatPlus.Common.Configs.ConfigElements;

public class PlayerColorConfigElement : ConfigElement<string>
{
    private Vector3 hsl;
    private string lastHex = "FFFFFF";

    private UIColoredImageButton copyButton;
    private UIColoredImageButton pasteButton;
    private UIColoredImageButton randomizeButton;

    private CustomColoredSlider hueSlider;
    private CustomColoredSlider saturationSlider;
    private CustomColoredSlider luminanceSlider;

    public string LiveValue => Value;
    public static PlayerColorConfigElement Instance { get; private set; }

    public override void OnBind()
    {
        base.OnBind();
        Instance = this;
        var hex = NormalizeHex(Value);
        if (!TryParseHex(hex, out var c)) c = Color.White;
        hsl = Main.rgbToHsl(c);
        lastHex = PlayerColorHandler.ColorToHex(c);
        Value = lastHex;

        MinHeight.Set(100, 0);
        Height.Set(100, 0);
    }

    public override void OnInitialize()
    {
        MinHeight.Set(100, 0);
        Height.Set(100, 0);

        // Make buttons
        copyButton = MakeButton("Copy", Loc.Get("ConfigPlayerColor.Copy"), 0, Copy);
        pasteButton = MakeButton("Paste", Loc.Get("ConfigPlayerColor.Paste"), 36, Paste);
        randomizeButton = MakeButton("Randomize", Loc.Get("ConfigPlayerColor.Randomize"), 36 * 2, Randomize);

        // Make sliders
        hueSlider = MakeHslSlider(HSLSliderId.Hue, 0);
        saturationSlider = MakeHslSlider(HSLSliderId.Saturation, 30);
        luminanceSlider = MakeHslSlider(HSLSliderId.Luminance, 60);
    }

    private void Copy()
    {
        var clip = Platform.Get<IClipboard>();
        if (clip != null)
            clip.Value = "#" + CurrentHex();
    }

    private void Paste()
    {
        var s = Platform.Get<IClipboard>()?.Value ?? string.Empty;
        ApplyHex(NormalizeHex(s));
    }

    public static string[] PlayerColors =
    [
        "F43030", // 1 red
        "03BF29", // 2 orange
        "ffff00", // 3 yellow
        "38ff00", // 4 green
        "00ffa7", // 5 cyan
        "0087ff", // 6 blue
        "7038ff", // 7 purple
        "ff50BE", // 8 pink
    ];

    private int lastIndex = -1; // store last chosen index

    private void Randomize_True()
    {
        // pick random r,g,b between 0 to 255
        var r = Main.rand.Next(0, 255);
        var g = Main.rand.Next(0, 255);
        var b = Main.rand.Next(0, 255);

        Color c = new(r, g, b);

        // convert to HSL
        hsl = Main.rgbToHsl(c);
        PushFromHsl();
    }

    private void Randomize()
    {
        if (PlayerColors.Length == 0)
            return;

        int idx = Main.rand.Next(PlayerColors.Length);

        // if random rolled the same as last time, pick a neighbor
        if (idx == lastIndex && PlayerColors.Length > 1)
        {
            idx = (idx + 1) % PlayerColors.Length;
        }

        lastIndex = idx;
        string hex = PlayerColors[idx];

        // parse hex -> Color
        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);

        Color c = new(r, g, b);

        // convert to HSL
        hsl = Main.rgbToHsl(c);
        PushFromHsl();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        var hex = NormalizeHex(Value);
        if (hex != lastHex && TryParseHex(hex, out var c))
        {
            hsl = Main.rgbToHsl(c);
            lastHex = hex;
        }
    }

    private CustomColoredImageButton MakeButton(string assetName, string tooltip, float left, Action onClick)
    {
        var btn = new CustomColoredImageButton(
            texture: Main.Assets.Request<Texture2D>($"Images/UI/CharCreation/{assetName}"),
            tooltip: tooltip);
        btn.OnLeftMouseDown += (_, _) => onClick();
        btn.Top.Set(34, 0);
        btn.Left.Set(170 + left, 0);
        Append(btn);
        return btn;
    }

    public override void Draw(SpriteBatch sb)
    {
        base.Draw(sb);
        var dims = GetDimensions();
        Rectangle area = new((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);
        Color color = Main.hslToRgb(hsl);

        // Draw hex text
        string hexText = "#" + CurrentHex();
        Vector2 hexTextPos = new(area.X + 173, area.Y + 6);
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, hexText, hexTextPos, Color.White, 0f, Vector2.Zero, new Vector2(1.0f));

        // Draw player name
        string name = "PlayerName";
        if (Main.LocalPlayer != null)
            name = Main.LocalPlayer.name;
        else
            name = "Player";
        Vector2 playerNamePos = new(area.X + 173, area.Y + 72);
        if (name == "") name = "Player";
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, name, playerNamePos, color, 0f, Vector2.Zero, baseScale: new Vector2(0.8f));

        // Draw hover
        Rectangle hoverRect = new(area.X, area.Y, 170, (int)Height.Pixels);
        bool hovered = hoverRect.Contains(Main.MouseScreen.ToPoint());

        // Tooltip assignment must never be null
        TooltipFunction = hovered
            ? () => "Set your own player color."
            : () => null;
    }

    private CustomColoredSlider MakeHslSlider(HSLSliderId id, int top)
    {
        Func<float> get = () => id == HSLSliderId.Hue ? hsl.X : id == HSLSliderId.Saturation ? hsl.Y : hsl.Z;
        Action<float> set = v => { if (id == HSLSliderId.Hue) hsl.X = v; else if (id == HSLSliderId.Saturation) hsl.Y = v; else hsl.Z = v; PushFromHsl(); };
        Func<float, Color> grad = x => id == HSLSliderId.Hue ? Main.hslToRgb(x, 1, 0.5f) : id == HSLSliderId.Saturation ? Main.hslToRgb(hsl.X, x, hsl.Z) : Main.hslToRgb(hsl.X, hsl.Y, x);
        CustomColoredSlider slider = new(LocalizedText.Empty, get, set, () => { }, grad, Color.Transparent)
        {
            Left = StyleDimension.FromPixels(5),
            Top = StyleDimension.FromPixels(top),
            Width = StyleDimension.FromPercent(1)
        };
        Append(slider);
        return slider;
    }

    private void PushFromHsl()
    {
        var c = Main.hslToRgb(hsl);
        var hex = PlayerColorHandler.ColorToHex(c);
        if (hex != lastHex)
        {
            lastHex = hex;
            Value = hex;

            // keep the synced table up to date
            PlayerColorSystem.PlayerColors[Main.myPlayer] = hex;

            // 🔁 invalidate mention caches for my name
            //MentionSnippet.InvalidateCachesFor(Main.LocalPlayer?.name);

            // if MP, announce update (you already use this elsewhere)
            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
                PlayerColorNetHandler.ClientHello(Main.myPlayer, hex);
        }
    }

    private void ApplyHex(string hex)
    {
        if (!TryParseHex(hex, out var c)) return;
        hsl = Main.rgbToHsl(c);
        var fixedHex = PlayerColorHandler.ColorToHex(c);
        lastHex = fixedHex;
        Value = fixedHex;
    }

    private static string NormalizeHex(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "FFFFFF";
        s = s.Trim();
        if (s.StartsWith("#", StringComparison.Ordinal)) s = s.Substring(1);
        if (s.Length != 6) return "FFFFFF";
        return s.ToUpperInvariant();
    }

    private static bool TryParseHex(string s, out Color c)
    {
        c = Color.White;
        if (string.IsNullOrWhiteSpace(s) || s.Length != 6) return false;
        if (!int.TryParse(s.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r)) return false;
        if (!int.TryParse(s.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g)) return false;
        if (!int.TryParse(s.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b)) return false;
        c = new Color(r, g, b);
        return true;
    }

    private string CurrentHex() => PlayerColorHandler.ColorToHex(Main.hslToRgb(hsl));
}
