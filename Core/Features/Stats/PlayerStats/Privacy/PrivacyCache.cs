using ChatPlus.Common.Configs;
using ChatPlus.Common.Debug;
using Terraria;

namespace ChatPlus.Core.Features.Stats.PlayerStats.StatsPrivacy;
public static class PrivacyCache
{
    static readonly Config.Privacy[] values = new Config.Privacy[Main.maxPlayers];

    static PrivacyCache()
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = Config.Privacy.Everyone;
        }
    }

    // debug function
    public static void PrintAll()
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var p = Main.player[i];
            if (p?.active == true)
            {
                var privacy = Get(i);
                Log.Info(p.name + ": " + privacy);
            }
        }
    }

    public static void Set(int whoAmI, Config.Privacy value)
    {
        if (whoAmI < 0) return;
        if (whoAmI >= values.Length) return;
        values[whoAmI] = value;
    }

    public static Config.Privacy Get(int whoAmI)
    {
        if (whoAmI < 0) return Config.Privacy.Everyone;
        if (whoAmI >= values.Length) return Config.Privacy.Everyone;
        return values[whoAmI];
    }
}
