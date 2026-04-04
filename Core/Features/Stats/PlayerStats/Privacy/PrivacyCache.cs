using ChatPlus.Common.Configs;
using ChatPlus.Common.Debug;
using Terraria;

namespace ChatPlus.Core.Features.Stats.PlayerStats.Privacy;
public static class PrivacyCache
{
    static readonly ClientConfig.StatsPrivacyMode[] values = new ClientConfig.StatsPrivacyMode[Main.maxPlayers];

    static PrivacyCache()
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = ClientConfig.StatsPrivacyMode.Everyone;
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

    public static void Set(int whoAmI, ClientConfig.StatsPrivacyMode value)
    {
        if (whoAmI < 0) return;
        if (whoAmI >= values.Length) return;
        values[whoAmI] = value;
    }

    public static ClientConfig.StatsPrivacyMode Get(int whoAmI)
    {
        if (whoAmI < 0) return ClientConfig.StatsPrivacyMode.Everyone;
        if (whoAmI >= values.Length) return ClientConfig.StatsPrivacyMode.Everyone;
        return values[whoAmI];
    }
}
