using ChatPlus.Core.Misc;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.BoldText;
public class ItalicsTagHandler : ITagHandler
{
    private class ItalicsSnippet : TextSnippet
    {
        public ItalicsSnippet(string text) : base(text) { }

        public override bool UniqueDraw(bool justCheckingString, out Vector2 size,
            SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1f)
        {
            DynamicSpriteFont italicsFont = FontSystem.Italics;

            //if (italicsFont == null)
                //italicsFont = FontAssets.MouseText.Value;

            if (justCheckingString)
            {
                size = italicsFont.MeasureString(Text) * scale;
                return true;
            }

            spriteBatch.DrawString(italicsFont, Text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            size = italicsFont.MeasureString(Text) * scale;
            return true;
        }
    }

    public static string GenerateTag(string text) => $"[italics:{text}]";

    public TextSnippet Parse(string text, Color baseColor = default, string options = null)
    {
        return new ItalicsSnippet(text) { Color = baseColor };
    }
}
