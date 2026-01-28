using ChatPlus.Common.Configs;
using ChatPlus.Common.Debug;
using ChatPlus.Core.Features.PlayerColors;
using ChatPlus.Core.Features.Stats.PlayerStats;
using ChatPlus.Core.Misc;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using static System.Net.Mime.MediaTypeNames;

namespace ChatPlus.Core.Features.Mentions;

public sealed class MentionSnippet : TextSnippet
{
    private bool isHovered;
    public int CurrentLineWidth;
    private readonly int _lineWidthUnscaled; // from tag

    // cache maps (case-insensitive)
    private static readonly Dictionary<string, string> _nameToHex = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, int> _nameToIndex = new(StringComparer.OrdinalIgnoreCase);

    // per-snippet last resolved info for click handling
    private int _lastIndex = -1;
    private string _lastName = string.Empty;

    public MentionSnippet(TextSnippet src, int lineWidthUnscaled = -1)
        : base(src.Text, src.Color, src.Scale)
    {
        CheckForHover = false; // pure highlight
        _lineWidthUnscaled = lineWidthUnscaled;
    }

    public static void InvalidateCachesFor(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        _nameToHex.Remove(name);
        _nameToIndex.Remove(name);
    }

    public static void ClearAllCaches()
    {
        _nameToHex.Clear();
        _nameToIndex.Clear();
    }

    private static int ResolveIndex(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return -1;

        if (_nameToIndex.TryGetValue(name, out int idx))
        {
            if ((uint)idx < Main.maxPlayers &&
                Main.player[idx]?.active == true &&
                string.Equals(Main.player[idx].name, name, StringComparison.OrdinalIgnoreCase))
                return idx;
        }

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var p = Main.player[i];
            if (p?.active == true && string.Equals(p.name, name, StringComparison.OrdinalIgnoreCase))
            {
                _nameToIndex[name] = i;
                return i;
            }
        }

        _nameToIndex[name] = -1;
        return -1;
    }

    private static string ResolveHex(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "FFFFFF";

        if (_nameToHex.TryGetValue(name, out var hex))
            return hex;

        // Prefer synced table
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var p = Main.player[i];
            if (p?.active == true &&
                string.Equals(p.name, name, StringComparison.OrdinalIgnoreCase) &&
                PlayerColorSystem.PlayerColors.TryGetValue(i, out var synced) &&
                !string.IsNullOrWhiteSpace(synced))
            {
                _nameToHex[name] = synced;
                return synced;
            }
        }

        // No synced color → default to white (not a hashed color)
        _nameToHex[name] = "FFFFFF";
        return "FFFFFF";
    }

    public override bool UniqueDraw(
        bool justCheckingString,
        out Vector2 size,
        SpriteBatch sb,
        Vector2 pos = default,
        Color passColor = default,
        float scale = 1f)
    {
        string name = Text ?? string.Empty;
        _lastName = name;
        _lastIndex = ResolveIndex(name);

        string playerName = "@" + name;

        var font = FontAssets.MouseText.Value;
        size = font.MeasureString(playerName) * scale;
        if (justCheckingString) return true;

        // skip shadow pass
        if (passColor.R + passColor.G + passColor.B <= 5)
            return true;

        // playerName color from cached hex
        string hex = ResolveHex(name);
        //Log.Info($"hex: {hex} for: {name}");
        Color playerColor = Color.White;
        if (hex.Length == 6 &&
            byte.TryParse(hex[..2], NumberStyles.HexNumber, null, out var r) &&
            byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, null, out var g) &&
            byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, null, out var b))
            playerColor = new Color(r, g, b);

        Vector2 p = new((float)Math.Floor(pos.X - 3), (float)Math.Floor(pos.Y));

        // measure size of snippet playerName
        size = font.MeasureString(playerName) * scale;

        if (string.Equals(name, Main.LocalPlayer.name, StringComparison.OrdinalIgnoreCase))
        {
            //DynamicSpriteFont boldFont = FontSystem.BoldFont;
            DynamicSpriteFont superboldFont = FontSystem.Bold;
            //Log.Info(playerColor + ", " + playerName);
            ChatManager.DrawColorCodedStringWithShadow(sb, superboldFont,
            playerName, p, playerColor, 0f, Vector2.Zero, Vector2.One);
            //Utils.DrawBorderString(sb, playerName, pos, playerColor);
        }
        else
            Utils.DrawBorderString(sb, playerName, pos, playerColor);

        // hover (acts like links)
        int width = (int)Math.Ceiling(size.X);
        int lineH = (int)Math.Ceiling(font.LineSpacing * scale);
        var hoverR = new Rectangle((int)p.X, (int)p.Y, width, Math.Max(1, lineH - 7));

        isHovered = hoverR.Contains(Main.MouseScreen.ToPoint());

        // to debug and always see hovered player overlay: comment the below line out
        if (isHovered)
        {
            Main.LocalPlayer.mouseInterface = true;

            // underline slightly darker than playerName color
            float dark = 0.35f;
            Color ul = new((byte)(playerColor.R * dark), (byte)(playerColor.G * dark), (byte)(playerColor.B * dark));
            int underlineY = (int)Math.Floor(p.Y + lineH - 10f);
            sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)p.X, underlineY, width, 2), ul);

            // Gate BOTH overlay and click
            bool allowInteraction = Conf.C.ShowStatsWhenHovering
                && (Conf.C.ShowStatsWhenBossIsAlive || !Main.CurrentFrameFlags.AnyActiveBossNPC);

            if (allowInteraction)
            {
                HoveredPlayerOverlay.Set(_lastIndex);

                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Log.Debug("click player");
                    OnClick();
                }
            }
        }

        return true;
    }

    //private void DrawYellowBackgroundHighlightRectangle(SpriteBatch sb, Rectangle rect)
    //{
    //    var font = FontAssets.MouseText.Value;
    //    int lineHeight = (int)Math.Ceiling(font.LineSpacing * 1f);
    //    int w = 300;
    //    //if (_lineWidthUnscaled > 0)
    //    //{
    //    //    w = _lineWidthUnscaled;
    //    //}
    //    //w = (int)font.MeasureString(Main.LocalPlayer.name).X + 25;
    //    //rect = new(
    //        //(int)p.X - 2,
    //        //(int)Math.Floor(pos.Y) + 1,
    //        //w - 7,
    //        //lineHeight - 9
    //    //);
    //    Color bgColor = new Color(250, 230, 160) * 0.7f;

    //    // load the rounded panel texture (31x31 with rounded edges)
    //    var tex = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel").Value;

    //    int c = 5; // corner size
    //    Rectangle corner = new(0, 0, c, c);
    //    Rectangle rightEdge = new(c, 0, tex.Width - 2 * c, c);
    //    Rectangle leftEdge = new(0, c, c, tex.Height - 2 * c);
    //    Rectangle center = new(c, c, tex.Width - 2 * c, tex.Height - 2 * c);

    //    //center fill
    //    sb.Draw(tex, new Rectangle(rect.X + c, rect.Y + c, rect.Width - 2 * c, rect.Height - 2 * c), center, bgColor);

    //    //edges
    //    sb.Draw(tex, new Rectangle(rect.X + c, rect.Y, rect.Width - 2 * c, c), rightEdge, bgColor);
    //    sb.Draw(tex, new Rectangle(rect.X + c, rect.Bottom - c, rect.Width - 2 * c, c), rightEdge, bgColor, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
    //    sb.Draw(tex, new Rectangle(rect.X, rect.Y + c, c, rect.Height - 2 * c), leftEdge, bgColor);
    //    sb.Draw(tex, new Rectangle(rect.Right - c, rect.Y + c, c, rect.Height - 2 * c), leftEdge, bgColor, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);

    //    // corners
    //    sb.Draw(tex, new Rectangle(rect.X, rect.Y, c, c), corner, bgColor);
    //    sb.Draw(tex, new Rectangle(rect.Right - c, rect.Y, c, c), corner, bgColor, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
    //    sb.Draw(tex, new Rectangle(rect.Right - c, rect.Bottom - c, c, c), corner, bgColor, 0, Vector2.Zero, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0);
    //    sb.Draw(tex, new Rectangle(rect.X, rect.Bottom - c, c, c), corner, bgColor, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
    //}

    public override void OnClick()
    {
        if (!Conf.C.ShowStatsWhenHovering)
            return;

        if (!Conf.C.ShowStatsWhenBossIsAlive && Main.CurrentFrameFlags.AnyActiveBossNPC)
            return;

        Main.LocalPlayer.mouseInterface = true;

        int idx = _lastIndex;
        if (idx < 0 || idx >= Main.maxPlayers)
            return;

        var target = Main.player[idx];
        if (target == null || !target.active)
            return;

        if (target != null && !PlayerInfoDrawer.HasAccess(Main.LocalPlayer, target))
        {
            //Main.NewText($"{target.name}'s stats is private.", Color.OrangeRed);
            //return;
        }

        var state = PlayerInfoState.instance;
        if (state == null)
        {
            Main.NewText("Player info UI not available.", Color.Orange);
            return;
        }

        state.SetPlayer(idx, target.name);

        Main.drawingPlayerChat = false;
        state.OpenForCurrentContext();
    }
}