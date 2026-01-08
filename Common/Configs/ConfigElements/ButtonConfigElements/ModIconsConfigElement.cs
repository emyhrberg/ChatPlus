using ChatPlus.Core.Features.ModIcons;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace ChatPlus.Common.Configs.ConfigElements.ButtonConfigElements;

public class ModIconsConfigElement : BaseBoolConfigElement
{
    protected override void OnToggled(bool newValue)
    {
        Conf.C.ShowModIconButton = newValue;
    }

    protected override void DrawPreview(SpriteBatch sb)
    {
        if (!Value) return;

        var dims = GetDimensions();
        Vector2 pos = new(dims.X + 175 - 3, dims.Y);

        string tag = ModIconTagHandler.GenerateTag("Terraria");

        // Draw the icon
        TextSnippet[] snippets = ChatManager.ParseMessage(tag, Color.White).ToArray();
        ChatManager.DrawColorCodedStringWithShadow(
            sb,
            FontAssets.MouseText.Value,
            snippets,
            pos + new Vector2(3, 4),
            0f,
            Vector2.Zero,
            Vector2.One,
            out int hovered
        );

        //// Show tooltip if hovered (doesnt work)
        //if (hovered >= 0)
        //{
        //    if (ModLoader.TryGetMod("ChatPlus", out Mod m))
        //    {
        //        HoveredModOverlay.Set(m);
        //    }
        //}
    }
}
