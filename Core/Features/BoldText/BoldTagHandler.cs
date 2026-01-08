using ChatPlus.Core.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.BoldText;
public class BoldTagHandler : ITagHandler
{
    private class BoldSnippet : TextSnippet
    {
        public BoldSnippet(string text) : base(text) { }

        public override bool UniqueDraw(bool justCheckingString, out Vector2 size,
            SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1f)
        {
            DynamicSpriteFont boldFont = FontSystem.Bold;

            position += new Vector2(0, 0f);

            if (justCheckingString)
            {
                size = boldFont.MeasureString(Text) * scale;
                return true;
            }

            spriteBatch.DrawString(boldFont, Text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            size = boldFont.MeasureString(Text) * scale;
            return true;
        }
    }

    public static string GenerateTag(string text) => $"[b:{text}]";

    public TextSnippet Parse(string text, Color baseColor = default, string options = null)
    {
        return new BoldSnippet(text) { Color = baseColor };
    }
}
