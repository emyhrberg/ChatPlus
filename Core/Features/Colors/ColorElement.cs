using System;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI.Chat;
using static ChatPlus.Common.Configs.Config;

namespace ChatPlus.Core.Features.Colors;
public class ColorElement : BaseElement<ColorEntry>
{
    public ColorEntry color;

    public ColorElement(ColorEntry color) : base(color)
    {
        this.color = color;
        Height.Set(30, 0);
        Width.Set(0, 1);
    }
    protected override void DrawListElement(SpriteBatch sb)
    {
        var dims = GetDimensions();
        Vector2 pos = dims.Position();
        string tag = color.Tag;

        tag = tag.Replace("[c/", "").Replace(":", "");
        Color previewColor = HexToColor(tag);

        const int boxSize = 24;
        var box = new Rectangle((int)pos.X + 4, (int)pos.Y + 3, boxSize, boxSize);

        sb.Draw(TextureAssets.MagicPixel.Value, box, previewColor);

        Color outline = Color.Black;
        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(box.X, box.Y, box.Width, 1), outline);
        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(box.X, box.Bottom - 1, box.Width, 1), outline);
        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(box.X, box.Y, 1, box.Height), outline);
        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(box.Right - 1, box.Y, 1, box.Height), outline);

        TextSnippet[] colorSnippet = [new TextSnippet(color.Tag)];
        ChatManager.DrawColorCodedStringWithShadow(
            sb,
            FontAssets.MouseText.Value,
            colorSnippet,
            pos + new Vector2(37, 4),
            0f,
            Vector2.Zero,
            Vector2.One,
            out _
        );
    }

    protected override void DrawGridElement(SpriteBatch sb)
    {
        var dims = GetDimensions();
        Vector2 pos = dims.Position();

        // Parse hex
        string tag = color.Tag.Replace("[c/", "").Replace(":", "");
        Color previewColor = HexToColor(tag);

        // Centered square swatch inside the cell
        const int padding = 4;
        int cellW = (int)dims.Width;
        int cellH = (int)dims.Height;
        int size = Math.Min(cellW, cellH) - padding * 2;
        if (size < 8)
        {
            size = 8;
        }

        int left = (int)pos.X + (cellW - size) / 2;
        int top = (int)pos.Y + (cellH - size) / 2;
        var box = new Rectangle(left, top, size, size);

        sb.Draw(TextureAssets.MagicPixel.Value, box, previewColor);

        // Thin black outline around swatch for contrast
        Color outline = Color.Black;
        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(box.X, box.Y, box.Width, 1), outline);
        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(box.X, box.Bottom - 1, box.Width, 1), outline);
        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(box.X, box.Y, 1, box.Height), outline);
        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(box.Right - 1, box.Y, 1, box.Height), outline);
    }

    private static Color HexToColor(string tag)
    {
        if (tag.Length == 6)
        {
            byte r = Convert.ToByte(tag[..2], 16);
            byte g = Convert.ToByte(tag.Substring(2, 2), 16);
            byte b = Convert.ToByte(tag.Substring(4, 2), 16);
            return new Color(r, g, b, 255);
        }
        return new Color(0, 0, 0, 0);
    }
}
