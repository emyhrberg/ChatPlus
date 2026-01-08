using System;
using System.Collections.Generic;
using ChatPlus.Common;
using ChatPlus.Core.Misc;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Stats.ModStats;

public static class ModInfoDrawer
{
    /// <summary>
    /// Draws a big tooltip panel with mod info, pokémon style.
    /// </summary>
    public static void Draw(SpriteBatch sb, Mod mod)
    {
        if (mod == null) return;

        // Dimensions
        const int panelWidth = 96;
        const int gutter = 10;
        const int side = 7;
        const int H = 356;
        const int W = panelWidth * 2 + gutter + side * 2;
        const int rowHeight = 31;
        var pos = Main.MouseScreen + new Vector2(20, 20);
        //textPos = new(300, 300); // debug
        pos.X = Math.Clamp(pos.X, 0, Main.screenWidth - W);
        pos.Y = Math.Clamp(pos.Y, 0, Main.screenHeight - H);
        var rect = new Rectangle((int)pos.X, (int)pos.Y, W, H);

        // Draw background and player
        DrawFullBGPanel(sb, rect);
        DrawModNameText(sb, rect, mod);
        Vector2 cursor = pos + new Vector2(side, 32);
        DrawHorizontalSeparator(sb, cursor, W - side * 2);
        cursor += new Vector2(-side, 10);
        rect = new Rectangle((W - 100) / 2, (int)cursor.Y, 100, 100);
        //DrawSeparatorBorder(sb, texRect);
        DrawBigModIcon(sb, cursor, W, mod);
        cursor += new Vector2(7, 110);
        DrawHorizontalSeparator(sb, cursor, W - side * 2);

        // Draw stats rows
        cursor += new Vector2(0, 14);
        int left = (int)cursor.X;

        // Draw rows
        DrawStat_ModSide(sb, new Rectangle(left, (int)cursor.Y, panelWidth, rowHeight), mod);
        DrawStat_FileSize(sb, new Rectangle(left, (int)cursor.Y + (rowHeight + 6) * 1, panelWidth, rowHeight), mod);
        DrawStat_LastUpdated(sb, new Rectangle(left, (int)cursor.Y + (rowHeight + 6) * 2, panelWidth, rowHeight), mod);
        DrawStat_Version(sb, new Rectangle(left, (int)cursor.Y + (rowHeight + 6) * 3, panelWidth, rowHeight), mod);
        DrawStat_Author(sb, new Rectangle(left, (int)cursor.Y + (rowHeight + 6) * 4, panelWidth, rowHeight), mod);
    }
    private static readonly Dictionary<string, string> _authorCache = new(StringComparer.OrdinalIgnoreCase);

    #region Stats

    public static void DrawStat_ModSide(SpriteBatch sb, Rectangle rect, Mod mod)
    {
        var statPanelTexture = Ass.StatPanel;
        var rect2 = new Rectangle(rect.X, rect.Y, statPanelTexture.Width() * 2 + 7, statPanelTexture.Height());
        sb.Draw(statPanelTexture.Value, rect2, Color.White);

        var sideTex = Ass.ServerIcon;
        if (mod.Side.ToString().Equals("Client", StringComparison.InvariantCultureIgnoreCase))
            sideTex = Ass.ClientIcon;

        sb.Draw(sideTex.Value, new Vector2(rect.X + 2, rect.Y + 5), Color.White);
        Utils.DrawBorderString(sb, $"Side: {mod.Side}", new Vector2(rect2.X + 28, rect2.Y + 5), Color.White);
    }

    public static void DrawStat_Author(SpriteBatch sb, Rectangle rect, Mod mod)
    {
        var statPanelTexture = Ass.StatPanel;
        var rect2 = new Rectangle(rect.X, rect.Y, statPanelTexture.Width() * 2 + 7, statPanelTexture.Height());
        sb.Draw(statPanelTexture.Value, rect2, Color.White);

        // icon
        sb.Draw(Ass.AuthorIcon.Value, new Vector2(rect.X, rect.Y + 5), Color.White);
        //sb.Draw(Ass.FileSizeIcon.Value, new Vector2(rect.X, rect.Y - 2), null, Color.White, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);


        // Resolve + cache author from build properties (no mod.Author in tML)
        if (!_authorCache.TryGetValue(mod.Name, out var authorText))
        {
            try
            {
                var localMod = ModHelper.GetLocalMod(mod);
                authorText = localMod?.properties?.author;
                if (string.IsNullOrWhiteSpace(authorText))
                    authorText = "Unknown";
            }
            catch
            {
                authorText = "Unknown";
            }
            _authorCache[mod.Name] = authorText;
        }

        if (authorText.Length > 9)
            authorText = authorText.Substring(0, 9) + "...";

        Utils.DrawBorderString(sb, $"Author: {authorText}", new Vector2(rect2.X + 28, rect2.Y + 5), Color.White);
    }

    public static void DrawStat_FileSize(SpriteBatch sb, Rectangle rect, Mod mod)
    {
        var statPanelTexture = Ass.StatPanel;
        var rect2 = new Rectangle(rect.X, rect.Y, statPanelTexture.Width() * 2 + 7, statPanelTexture.Height());
        sb.Draw(statPanelTexture.Value, rect2, Color.White);

        sb.Draw(Ass.FileSizeIcon.Value, new Vector2(rect.X - 5, rect.Y - 2), null, Color.White, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);

        // Uses ModMetaCache (cached per-mod; no re-scan each frame)
        var meta = ModMetaCache.Get(mod);
        string sizeText = meta.size > 0 ? $"{meta.size / 1024f / 1024f:0.00} MB" : "n/a";
        Utils.DrawBorderString(sb, $"Size: {sizeText}", new Vector2(rect2.X + 28, rect2.Y + 5), Color.White);
    }

    public static void DrawStat_LastUpdated(SpriteBatch sb, Rectangle rect, Mod mod)
    {
        var statPanelTexture = Ass.StatPanel;
        var rect2 = new Rectangle(rect.X, rect.Y, statPanelTexture.Width() * 2 + 7, statPanelTexture.Height());
        sb.Draw(statPanelTexture.Value, rect2, Color.White);

        var tex = Main.Assets.Request<Texture2D>("Images/UI/VK_Shift").Value;
        sb.Draw(tex, new Vector2(rect.X - 0, rect.Y + 1), Color.ForestGreen);

        var (size, lastWriteUtc, path) = ModMetaCache.Get(mod);
        string ago = lastWriteUtc == DateTime.MinValue
            ? "n/a"
            : ToAgoString(lastWriteUtc, DateTime.UtcNow);

        Utils.DrawBorderString(sb, $"Update: {ago}", new Vector2(rect2.X + 28, rect2.Y + 5), Color.White);
    }

    public static void DrawStat_Version(SpriteBatch sb, Rectangle rect, Mod mod)
    {
        var statPanelTexture = Ass.StatPanel;
        var rect2 = new Rectangle(rect.X, rect.Y, statPanelTexture.Width() * 2 + 7, statPanelTexture.Height());
        sb.Draw(statPanelTexture.Value, rect2, Color.White);

        //sb.Draw(Ass.VersionIcon.Value, new Vector2(rect.X - 2, rect.Y - 1), Color.White);
        sb.Draw(Ass.VersionIcon.Value, new Vector2(rect.X + 1, rect.Y + 3), null, Color.White, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);


        Utils.DrawBorderString(sb, $"Version: {mod.Version}", new Vector2(rect2.X + 28, rect2.Y + 5), Color.White);
    }

    private static string ToAgoString(DateTime whenUtc, DateTime nowUtc)
    {
        var span = nowUtc - whenUtc;
        if (span.TotalDays >= 1) return $"{(int)span.TotalDays} days ago";
        if (span.TotalHours >= 1) return $"{(int)span.TotalHours} hours ago";
        if (span.TotalMinutes >= 1) return $"{(int)span.TotalMinutes} minutes ago";
        return "just now";
    }

    #endregion Stats

    #region Helpers
    private static void DrawBigModIcon(SpriteBatch sb, Vector2 pos, int W, Mod mod)
    {
        if (mod == null) return;

        Texture2D tex = null;

        if (mod.Name == "ModLoader")
            tex = Ass.tModLoaderIcon.Value;

        // Priority: icon_workshop
        if (mod.FileExists("icon_workshop.rawimg"))
            tex = mod.Assets.Request<Texture2D>("icon_workshop").Value;
        else if (mod.FileExists("icon.png"))
            tex = mod.Assets.Request<Texture2D>("icon").Value;
        else if (mod.FileExists("icon_small.rawimg"))
            tex = mod.Assets.Request<Texture2D>("icon_small").Value;

        if (tex != null)
        {
            float scale = Math.Min(100 / (float)tex.Width, 100 / (float)tex.Height);
            pos += new Vector2((W - 100) / 2, 0);

            sb.Draw(tex, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            return;
        }

        // Fallback: draw two-letter initials centered
        string initials = mod.Name;
        if (!string.IsNullOrWhiteSpace(initials))
        {
            initials = initials.Length >= 2 ? initials[..2] : initials;
            Utils.DrawBorderString(sb, initials, pos, Color.White, 0.64f, 0.5f, 0.5f);
        }
    }

    private static void DrawModNameText(SpriteBatch sb, Rectangle rect, Mod mod)
    {
        if (mod == null) return;

        string modText = $"Mod: {mod.DisplayNameClean}";
        if (mod.displayNameClean.Length > 23)
            modText = mod.displayNameClean.Replace(mod.displayNameClean[23..], "..");

        var snippets = ChatManager.ParseMessage(modText, Color.White).ToArray();
        Vector2 textSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, modText, Vector2.One);
        float textWidth = textSize.X;
        Vector2 pos = new(rect.X + (rect.Width - textWidth) / 2f, rect.Y + 6);
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, snippets, pos, 0f, Vector2.Zero, Vector2.One, out _);
    }
    #endregion

    #region Panels & Separators
    public static void DrawSeparatorBorder(SpriteBatch sb, Rectangle rect, int edgeWidth = 2)
    {
        var tex = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Separator1").Value;
        Color color = new Color(89, 116, 213, 255) * 0.9f;

        DrawPanel(tex, edgeWidth, 0, sb, new Vector2(rect.X, rect.Y), rect.Width, color);
        DrawPanel(tex, edgeWidth, 0, sb, new Vector2(rect.X, rect.Bottom - edgeWidth), rect.Width, color);
        DrawPanelVertical(tex, edgeWidth, 0, sb, new Vector2(rect.X, rect.Y), rect.Height, color);
        DrawPanelVertical(tex, edgeWidth, 0, sb, new Vector2(rect.Right - edgeWidth, rect.Y), rect.Height, color);
    }

    public static void DrawPanel(Texture2D texture, int edgeWidth, int edgeShove, SpriteBatch spriteBatch, Vector2 position, float width, Color color)
    {
        spriteBatch.Draw(texture,
            new Vector2(position.X + edgeWidth, position.Y), new Rectangle(edgeWidth + edgeShove, 0, texture.Width - (edgeWidth + edgeShove) * 2, texture.Height),
            color,
            0f,
            Vector2.Zero,
            new Vector2((width - edgeWidth * 2) / (texture.Width - (edgeWidth + edgeShove) * 2), 1f),
            SpriteEffects.None,
            0f);
    }

    public static void DrawPanelVertical(Texture2D texture, int edgeWidth, int edgeShove, SpriteBatch spriteBatch, Vector2 position, float height, Color color)
    {
        spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y + edgeWidth, edgeWidth, (int)height - edgeWidth * 2),
            new Rectangle(texture.Width / 2, 0, 1, texture.Height), color);
    }

    public static void DrawFullBGPanel(SpriteBatch sb, Rectangle rect)
    {
        var BG = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
        var Border = Main.Assets.Request<Texture2D>("Images/UI/PanelBorder");

        const int corner = 12;
        const int bar = 4;

        static void DrawNineSlice(SpriteBatch s, Texture2D tex, Rectangle dst, Color col)
        {
            int w = Math.Max(dst.Width, corner * 2 + 1);
            int h = Math.Max(dst.Height, corner * 2 + 1);
            int x0 = dst.X, y0 = dst.Y;
            int x1 = x0 + corner, y1 = y0 + corner;
            int x2 = x0 + w - corner, y2 = y0 + h - corner;
            s.Draw(tex, new Rectangle(x0, y0, corner, corner), new Rectangle(0, 0, corner, corner), col);
            s.Draw(tex, new Rectangle(x2, y0, corner, corner), new Rectangle(corner + bar, 0, corner, corner), col);
            s.Draw(tex, new Rectangle(x0, y2, corner, corner), new Rectangle(0, corner + bar, corner, corner), col);
            s.Draw(tex, new Rectangle(x2, y2, corner, corner), new Rectangle(corner + bar, corner + bar, corner, corner), col);
            s.Draw(tex, new Rectangle(x1, y0, w - corner * 2, corner), new Rectangle(corner, 0, bar, corner), col);
            s.Draw(tex, new Rectangle(x1, y2, w - corner * 2, corner), new Rectangle(corner, corner + bar, bar, corner), col);
            s.Draw(tex, new Rectangle(x0, y1, corner, h - corner * 2), new Rectangle(0, corner, corner, bar), col);
            s.Draw(tex, new Rectangle(x2, y1, corner, h - corner * 2), new Rectangle(corner + bar, corner, corner, bar), col);
            s.Draw(tex, new Rectangle(x1, y1, w - corner * 2, h - corner * 2), new Rectangle(corner, corner, bar, bar), col);
        }

        DrawNineSlice(sb, BG.Value, rect, new Color(63, 82, 151) * 1.0f);
        DrawNineSlice(sb, Border.Value, rect, Color.Black);
    }

    public static void DrawHorizontalSeparator(SpriteBatch sb, Vector2 pos, float width, int edgeWidth = 2)
    {
        var tex = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Separator1").Value;
        Color color = new Color(89, 116, 213, 255) * 0.9f;
        DrawPanel(tex, edgeWidth, 0, sb, pos, width, color);
    }
    #endregion
    private static void DrawDebugRect(Rectangle r) => Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, r, Color.Red * 0.5f);
}

