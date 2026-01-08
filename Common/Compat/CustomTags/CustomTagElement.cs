using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.UI;

namespace ChatPlus.Common.Compat.CustomTags;
public sealed class CustomTagElement : BaseElement<CustomTag>
{
    private readonly UIElement view;
    private readonly string label;

    private const int padding = 8;
    private const int fallbackThumbSize = 24;
    private const int stroke = 1;

    public CustomTagElement(CustomTag data) : base(data)
    {
        view = data.DisplayElement;
        label = data.ActualTag;

        if (view != null)
        {
            view.HAlign = 0f;
            view.VAlign = 0.5f;
            view.Left.Set(padding, 0f);
            Append(view);
        }
    }

    protected override void DrawGridElement(SpriteBatch sb)
    {
        CustomDraw(sb);
    }

    protected override void DrawListElement(SpriteBatch sb)
    {
        CustomDraw(sb);
    }

    private void CustomDraw(SpriteBatch sb)
    {
        var dims = GetDimensions();

        float textX = dims.X + padding;
        float centerY = dims.Y + dims.Height * 0.5f;

        if (view != null)
        {
            var vd = view.GetOuterDimensions();
            float right = vd.X + vd.Width;
            textX = right + padding;
        }
        else
        {
            // Fallback thumbnail when no UIElement is provided
            int x = (int)(dims.X + padding);
            int y = (int)(centerY - fallbackThumbSize * 0.5f);

            var fill = new Rectangle(x, y, fallbackThumbSize, fallbackThumbSize);
            var border = TextureAssets.MagicPixel.Value;

            // Fill
            sb.Draw(border, fill, new Color(48, 56, 96));

            // Stroke
            for (int i = 0; i < stroke; i++)
            {
                sb.Draw(border, new Rectangle(fill.X, fill.Y + i, fill.Width, 1), Color.Black);
                sb.Draw(border, new Rectangle(fill.X, fill.Bottom - 1 - i, fill.Width, 1), Color.Black);
                sb.Draw(border, new Rectangle(fill.X + i, fill.Y, 1, fill.Height), Color.Black);
                sb.Draw(border, new Rectangle(fill.Right - 1 - i, fill.Y, 1, fill.Height), Color.Black);
            }

            textX = fill.Right + padding;
        }

        var font = FontAssets.MouseText.Value;
        var size = font.MeasureString(label);
        float textY = centerY - size.Y * 0.5f;

        sb.DrawString(font, label, new Vector2(textX, textY), Color.White);
    }

    public override void Draw(SpriteBatch sb)
    {
        base.Draw(sb); // selection background + children

        CustomDraw(sb);
    }
}
