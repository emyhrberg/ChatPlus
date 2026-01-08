using System;
using ChatPlus.Common.Debug;
using MonoMod.Cil;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.PlayerIcons

{
    /// <summary>
    /// Modifies the DrawPlayerHead method in the MapHeadRenderer class to force the player head icon to be drawn facing right.
    /// The default behaviour is to draw the head icon facing the direction of the player, so this is overriding that.
    /// </summary>
    public class MapHeadRendererHook : ModSystem
    {
        public static bool shouldFlipHeadDraw = true;

        public override void Load()
        {
            IL_MapHeadRenderer.DrawPlayerHead += IL_MapHeadRenderer_DrawPlayerHead;
        }

        public static void IL_MapHeadRenderer_DrawPlayerHead(ILContext il)
        {
            try
            {
                ILCursor c = new(il);

                // extra code
                c.GotoNext(MoveType.Before, i => i.MatchLdcR4(2));
                c.Index += 2;

                ILLabel skipCentering = il.DefineLabel();

                c.EmitLdsfld(typeof(MapHeadRendererHook).GetField(nameof(shouldFlipHeadDraw)));
                c.EmitBrfalse(skipCentering);
                c.EmitDelegate<Func<Vector2, Vector2>>(inCenter =>
                {
                    // return new Vector2(inCenter.X * 0.8f, inCenter.Y); // original, almost works but 0.1f offset. should go more left than this.
                    return new Vector2(inCenter.X - 8f, inCenter.Y); // orig
                });
                c.MarkLabel(skipCentering);

                // find where the draw data loads the 
                c.GotoNext(MoveType.After, i => i.MatchLdcI4(0));
                c.EmitLdsfld(typeof(MapHeadRendererHook).GetField(nameof(shouldFlipHeadDraw)));
                c.EmitXor();
            }
            catch (Exception e)
            {
                Log.Info("Error in IL_MapHeadRenderer_DrawPlayerHead: " + e.Message);
            }
        }
    }
}
