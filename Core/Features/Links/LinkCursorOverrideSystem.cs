using ChatPlus.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.Links;

[Autoload(Side = ModSide.Client)]
internal sealed class LinkCursorOverrideSystem : ModSystem
{
    public static bool WantLinkCursor;

    private static Asset<Texture2D> _cursor;

    public override void Load()
    {
        _cursor = Ass.LinkCursor;
        On_Main.DrawInterface_36_Cursor += DrawInterface_36_Cursor;
    }

    public override void Unload()
    {
        On_Main.DrawInterface_36_Cursor -= DrawInterface_36_Cursor;
        _cursor = null;
        WantLinkCursor = false;
    }

    //public override void PostUpdateInput()
    //{
        //    WantLinkCursor = false;
    //}

    private static void DrawInterface_36_Cursor(On_Main.orig_DrawInterface_36_Cursor orig)
    {
        if (!WantLinkCursor || _cursor == null || !_cursor.IsLoaded)
        {
            orig();
            return;
        }

        // Restart spritebatch
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.SamplerStateForCursor, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

        Texture2D tex = _cursor.Value;
        Vector2 pos = new Vector2(Main.mouseX, Main.mouseY);

        // debug: draw both. do not delete
        //pos.X += 30;

        // Shadow
        Color shadow = new Color((int)(Main.cursorColor.R * 0.2f), (int)(Main.cursorColor.G * 0.2f), (int)(Main.cursorColor.B * 0.2f), (int)(Main.cursorColor.A * 0.5f));
        Main.spriteBatch.Draw(tex, pos + Vector2.One, null, shadow, 0f, Vector2.Zero, Main.cursorScale, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(tex, pos, null, Color.White, 0f, Vector2.Zero, Main.cursorScale, SpriteEffects.None, 0f);
    }
}
