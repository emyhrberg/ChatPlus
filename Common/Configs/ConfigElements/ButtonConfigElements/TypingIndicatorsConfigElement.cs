using ChatPlus.Core.Features.PlayerColors;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace ChatPlus.Common.Configs.ConfigElements.ButtonConfigElements;

public class TypingIndicatorsConfigElement : BaseBoolConfigElement
{
    protected override void OnToggled(bool newValue)
    {
        Conf.C.TypingIndicators = newValue;
    }

    protected override void DrawPreview(SpriteBatch sb)
    {
        if (!Value) return;

        var dims = GetDimensions();
        Vector2 pos = new(dims.X + 175 - 3, dims.Y+3);


        var tex = Ass.TypingIndicator; // w: 32, h: 26, frames: 10
        int frame = 1; 
        Rectangle rect = new(32 * frame, 0, 32, 26);
        sb.Draw(tex.Value, pos, rect, Color.White, 0f, Vector2.Zero, 0.9f, 0f, 0f);
    }
}