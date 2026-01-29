using ChatPlus.Common.Configs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.Stats.PlayerStats.Privacy;

public class PrivacyPlayer : ModPlayer
{
    public override void OnEnterWorld()
    {
        var privacy = Conf.C.StatsPrivacy;
        PrivacyCache.Set(Player.whoAmI, privacy);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            PrivacyNetHandler.SendLocalPrivacy();
        }

        if (Main.netMode == NetmodeID.Server)
        {
            PrivacyNetHandler.ServerSyncTo(Player.whoAmI);
        }
    }
}
