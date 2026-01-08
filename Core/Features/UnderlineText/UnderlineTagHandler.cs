using System;
using ChatPlus.Core.Misc;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.UnderlineText;
public class UnderlineTagHandler : ITagHandler
{
    private class UnderlineSnippet : TextSnippet
    {
        public UnderlineSnippet(string text) : base(text) { }

        public override bool UniqueDraw(bool justCheckingString, out Vector2 size,
            SpriteBatch sb, Vector2 pos = default, Color color = default, float scale = 1f)
        {
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            size = font.MeasureString(Text) * scale;
            if (justCheckingString)
                return true;

            // skip shadow pass
            if (color.R + color.G + color.B <= 5)
                return true;

            // draw text
            //sb.DrawString(font, Text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            DrawHelper.DrawText(sb, Text, pos, color);

            // underline rect
            Vector2 p = new((float)Math.Floor(pos.X), (float)Math.Floor(pos.Y));
            int width = (int)Math.Ceiling(size.X);
            int lineHeight = (int)Math.Ceiling(font.LineSpacing * scale);
            int underlineY = (int)Math.Floor(p.Y + lineHeight - 10);

            var underlineRect = new Rectangle((int)p.X, underlineY, width, 2);
            sb.Draw(TextureAssets.MagicPixel.Value, underlineRect, color);

            return true;
        }
    }

    public static string GenerateTag(string text) => $"[underline:{text}]";

    public TextSnippet Parse(string text, Color baseColor = default, string options = null)
    {
        return new UnderlineSnippet(text) { Color = baseColor };
    }
}

