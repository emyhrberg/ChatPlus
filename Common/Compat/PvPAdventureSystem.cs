using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace ChatPlus.Common.Compat;

/// <summary>
/// System that integrates with PvPAdventure features:
/// </summary>
internal class PvPAdventureSystem : ModSystem
{
    //private Hook newTextHook;
    //private Hook renderHook;

    //private delegate void newTextOrig(object self, string text, bool force, Color c, int widthLimit);
    //private delegate void RenderChatOrig(object self, bool extendedChatWindow);

    public override void Load()
    {
        if (ModLoader.TryGetMod("PvPAdventure", out Mod cc))
        {
            //InitializeNewTextHook(cc);
            //InitializeRenderChatHook(cc);
        }
    }
}
