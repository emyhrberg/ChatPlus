using System;
using System.Collections.Generic;
using ChatPlus.Common.Debug;
using Terraria;

namespace ChatPlus.Core.Features.Stats.PlayerStats.SessionTracker;

/// <summary>
/// Interface for tracking player session start times.
/// </summary>
internal interface ISessionTracker
{
    void Update();
    void Clear();
    void OnJoin(int playerIndex, DateTime start);
    void OnLeave(int playerIndex);
    bool TryGetStartTime(int playerIndex, out DateTime start);
}

internal class SinglePlayerSessionTracker : ISessionTracker
{
    /// <summary>
    /// Stores player index as keys and their session start time as values.
    /// </summary>
    private readonly Dictionary<int, DateTime> sessions = [];

    public void Update()
    {
        var player = Main.LocalPlayer;
        if (!sessions.ContainsKey(player.whoAmI) && player.active)
        {
            sessions[player.whoAmI] = DateTime.UtcNow;
        }
    }

    public void Clear() => sessions.Clear();
    public void OnJoin(int playerIndex, DateTime start) => sessions[playerIndex] = start;
    public void OnLeave(int playerIndex) => sessions.Remove(playerIndex);
    public bool TryGetStartTime(int playerIndex, out DateTime start) => sessions.TryGetValue(playerIndex, out start);
}

internal class MultiPlayerSessionTracker : ISessionTracker
{
    /// <summary>
    /// Stores player index as keys and their session start time as values.
    /// </summary>
    private readonly Dictionary<int, DateTime> sessions = [];

    public void Update()
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var plr = Main.player[i];
            if (plr.active)
            {
                if (!sessions.ContainsKey(i))
                {
                    DateTime joinTime = DateTime.UtcNow;
                    sessions[i] = joinTime;
                    Log.Info("Player joined: " + plr.name + " at " + joinTime.ToString("HH:mm:ss"));
                    //SessionTrackerSystem.SendPlayerJoin(i, joinTime);
                    //SessionTrackerSystem.SendFullSync(i);
                }
            }
            else
            {
                //Log.Info("Player left: " + plr.name);
                //if (sessions.Remove(i))
                //SessionTrackerSystem.SendPlayerLeave(i);
            }
        }
    }
    public void Clear() => sessions.Clear();
    public void OnJoin(int playerIndex, DateTime start) => sessions[playerIndex] = start;
    public void OnLeave(int playerIndex) => sessions.Remove(playerIndex);
    public bool TryGetStartTime(int playerIndex, out DateTime start) => sessions.TryGetValue(playerIndex, out start);
}
