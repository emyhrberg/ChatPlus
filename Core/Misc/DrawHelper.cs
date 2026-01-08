using ChatPlus.Common;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Misc;

public static class DrawHelper
{
    /// <summary>
    /// Draw text with less boilerplate.
    /// Only spritebatch, text, and position is required.
    /// </summary>
    public static void DrawText(SpriteBatch sb, string text, Vector2 pos, Color color = default, Vector2 scale = default, DynamicSpriteFont font = default)
    {
        if (scale == default) scale = Vector2.One;
        if (color == default) color = Color.White;
        if (font == default) font = FontAssets.MouseText.Value;

        ChatManager.DrawColorCodedStringWithShadow(
            spriteBatch: sb, 
            font: font, 
            text: text, 
            position: pos, 
            baseColor: color, 
            rotation: 0f, 
            origin: Vector2.Zero, 
            baseScale: scale);
    }

    public static void DrawFill(SpriteBatch sb, UIElement ele = null, Rectangle rect = default)
    {
        Rectangle t = new(0, 0, 0, 0);

        // Dimensions
        if (ele != null)
        {
            CalculatedStyle dims = ele.GetDimensions();
            t = new((int)dims.X + 4, (int)dims.Y + 4, (int)dims.Width - 8, (int)dims.Height - 6);
        }
        if (rect != default)
        {
            t = rect;
        }


        Texture2D pixel = TextureAssets.MagicPixel.Value;

        // fill (slightly brighter blue, semi-transparent)
        sb.Draw(pixel, t, new Color(70, 120, 220, 140));

        // white border
        const int b = 0;
        sb.Draw(pixel, new Rectangle(t.X, t.Y, t.Width, b), Color.White);                 // top
        sb.Draw(pixel, new Rectangle(t.X, t.Bottom - b, t.Width, b), Color.White);        // bottom
        sb.Draw(pixel, new Rectangle(t.X, t.Y, b, t.Height), Color.White);                // left
        sb.Draw(pixel, new Rectangle(t.Right - b, t.Y, b, t.Height), Color.White);        // right
    }

    public static void DrawSlices(SpriteBatch sb, UIElement ele = null, Rectangle rect = default, int extraSize = 0)
    {
        Rectangle t = new(0, 0, 0, 0);

        if (ele != null)
            t = ele.GetDimensions().ToRectangle();
        if (rect != default)
            t = rect;

        // expand rectangle outward, centered
        if (extraSize > 0)
        {
            t.Inflate(extraSize, extraSize);
        }

        var tex = Ass.Hitbox.Value;
        int c = 5;
        Rectangle sc = new(0, 0, c, c),
                  eh = new(c, 0, 30 - 2 * c, c),
                  ev = new(0, c, c, 30 - 2 * c),
                  ce = new(c, c, 30 - 2 * c, 30 - 2 * c);

        Color color = Color.White;

        // Fill
        sb.Draw(tex, new Rectangle(t.X + c, t.Y + c, t.Width - 2 * c, t.Height - 2 * c), ce, color * 0.3f);

        // Edges
        sb.Draw(tex, new Rectangle(t.X + c, t.Y, t.Width - 2 * c, c), eh, color);
        sb.Draw(tex, new Rectangle(t.X + c, t.Bottom - c, t.Width - 2 * c, c), eh, color, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
        sb.Draw(tex, new Rectangle(t.X, t.Y + c, c, t.Height - 2 * c), ev, color);
        sb.Draw(tex, new Rectangle(t.Right - c, t.Y + c, c, t.Height - 2 * c), ev, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);

        // Corners
        sb.Draw(tex, new Rectangle(t.X, t.Y, c, c), sc, color);
        sb.Draw(tex, new Rectangle(t.Right - c, t.Y, c, c), sc, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
        sb.Draw(tex, new Rectangle(t.Right - c, t.Bottom - c, c, c), sc, color, 0, Vector2.Zero, SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally, 0);
        sb.Draw(tex, new Rectangle(t.X, t.Bottom - c, c, c), sc, color, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
    }

    public static void DrawPixelatedBorder(SpriteBatch sb, Rectangle r, Color c, int size = 2, int stroke = 1)
    {
        var t = TextureAssets.MagicPixel.Value;

        // --- Edges (minus the corners) ---
        for (int s = 0; s < stroke; s++)
        {
            // Top
            sb.Draw(t, new Rectangle(r.X + size, r.Y + s, r.Width - size * 2, 1), c);
            // Bottom
            sb.Draw(t, new Rectangle(r.X + size, r.Bottom - 1 - s, r.Width - size * 2, 1), c);
            // Left
            sb.Draw(t, new Rectangle(r.X + s, r.Y + size, 1, r.Height - size * 2), c);
            // Right
            sb.Draw(t, new Rectangle(r.Right - 1 - s, r.Y + size, 1, r.Height - size * 2), c);
        }

        // --- Pixelated corners ---
        for (int i = 0; i < size; i++)
        {
            for (int s = 0; s < stroke; s++)
            {
                // Top-left
                sb.Draw(t, new Rectangle(r.X + i, r.Y + s, 1, i + 1), c);
                sb.Draw(t, new Rectangle(r.X + s, r.Y + i, i + 1, 1), c);

                // Top-right
                sb.Draw(t, new Rectangle(r.Right - 1 - i, r.Y + s, 1, i + 1), c);
                sb.Draw(t, new Rectangle(r.Right - 1 - s, r.Y + i, i + 1, 1), c);

                // Bottom-left
                sb.Draw(t, new Rectangle(r.X + i, r.Bottom - 1 - s, 1, i + 1), c);
                sb.Draw(t, new Rectangle(r.X + s, r.Bottom - 1 - i, i + 1, 1), c);

                // Bottom-right
                sb.Draw(t, new Rectangle(r.Right - 1 - i, r.Bottom - 1 - s, 1, i + 1), c);
                sb.Draw(t, new Rectangle(r.Right - 1 - s, r.Bottom - 1 - i, i + 1, 1), c);
            }
        }
    }
}
