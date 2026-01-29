using ChatPlus.Common.Configs;
using ChatPlus.Common.Debug;
using Terraria;

namespace ChatPlus.Core.Features.Stats.PlayerStats.Privacy;
public static class PrivacyCache
{
    static readonly Config.StatsPrivacyMode[] values = new Config.StatsPrivacyMode[Main.maxPlayers];

    static PrivacyCache()
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = Config.StatsPrivacyMode.Everyone;
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

    public static void Set(int whoAmI, Config.StatsPrivacyMode value)
    {
        if (whoAmI < 0) return;
        if (whoAmI >= values.Length) return;
        values[whoAmI] = value;
    }

    public static Config.StatsPrivacyMode Get(int whoAmI)
    {
        if (whoAmI < 0) return Config.StatsPrivacyMode.Everyone;
        if (whoAmI >= values.Length) return Config.StatsPrivacyMode.Everyone;
        return values[whoAmI];
    }
}
