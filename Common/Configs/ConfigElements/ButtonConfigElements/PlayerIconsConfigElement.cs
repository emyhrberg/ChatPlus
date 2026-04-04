using ChatPlus.Core.Features.PlayerIcons;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace ChatPlus.Common.Configs.ConfigElements.ButtonConfigElements;

public class PlayerIconsConfigElement : BaseBoolConfigElement
{
    protected override void OnToggled(bool newValue)
    {
        Conf.C.ShowPlayerIconButton = newValue;
    }

    protected override void DrawPreview(SpriteBatch sb)
    {
        if (!Value) return;

        var dims = GetDimensions();
        Vector2 pos = new(dims.X + 175 + 3, dims.Y + 11);
        Rectangle rect = new((int)pos.X - 8, (int)pos.Y - 6, 22, 22);

        var player = Main.LocalPlayer;

        // Fallback if no player found
        if (player == null || !player.active || player.name == "" || Main.gameMenu)
        {
            var guide = Ass.AuthorIcon.Value;
            rect.X += 2;
            sb.Draw(guide, rect, Color.White);
            return;
        }

        try
        {
            FlipMapHeadRendererHook.shouldFlipHeadDraw = player.direction == -1;
            Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, player, pos, 1f, 0.75f, Color.White);
        }
        finally
        {
            FlipMapHeadRendererHook.shouldFlipHeadDraw = false;
        }
    }
}