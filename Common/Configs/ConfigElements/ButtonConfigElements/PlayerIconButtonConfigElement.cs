using ChatPlus.Core.Chat.ChatButtons.Shared;
using Microsoft.Xna.Framework.Graphics;

namespace ChatPlus.Common.Configs.ConfigElements.ButtonConfigElements;

internal class PlayerIconButtonConfigElement : BaseBoolConfigElement
{
    protected override void OnToggled(bool newValue)
    {
        if (MemberInfo != null && Item != null)
        {
            MemberInfo.SetValue(Item, newValue);
        }
    }

    protected override void DrawPreview(SpriteBatch sb)
    {
        var dims = GetDimensions();
        Vector2 pos = new(dims.X + 175 - 7, dims.Y + 4);
        ChatButtonRenderer.Draw(sb, ChatButtonType.PlayerIcons, pos, 24, grayscale: !Value);
    }
}
