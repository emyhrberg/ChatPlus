using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.Stats.PlayerStats.SessionTracker;

public partial class SessionTrackerSystem : ModSystem
{
    /// <summary>
    /// Determines whether we are tracking sessions in singleplayer or multiplayer.
    /// </summary>
    private static ISessionTracker tracker;

    public override void OnWorldLoad()
    {
        tracker = Main.netMode == NetmodeID.SinglePlayer
            ? new SinglePlayerSessionTracker()
            : new MultiPlayerSessionTracker();
    }

    public override void OnWorldUnload()
    {
        tracker?.Clear();
        tracker = null;
    }

    public override void PostUpdatePlayers()
    {
        tracker?.Update();
    }

    public static string GetSessionDurationIngameDays(int playerIndex)
    {
        if (!tracker.TryGetStartTime(playerIndex, out DateTime start))
            return string.Empty;

        var elapsed = DateTime.UtcNow - start;
        const double RealSecondsPerIngameDay = 24 * 60;
        double days = elapsed.TotalSeconds / RealSecondsPerIngameDay;
        return $"{days:0.00}";
    }

    public static string GetSessionDuration(int playerIndex)
    {
        if (!tracker.TryGetStartTime(playerIndex, out DateTime start))
            return string.Empty;

        var span = DateTime.UtcNow - start;

        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";

        return $"{(int)span.TotalMinutes:D2}:{span.Seconds:D2}";
    }
}
