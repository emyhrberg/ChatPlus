using ChatPlus.Common.Debug;
using ChatPlus.Core.Misc;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.TypingIndicators;

internal static class TypingIndicatorNetHandler
{
    public static void Receive(BinaryReader reader, int fromWho)
    {
        int playerId = reader.ReadInt32();
        bool isTyping = reader.ReadBoolean();
        int team = reader.ReadInt32();

        TypingIndicatorSystem.TypingPlayers[playerId] = isTyping;
        TypingIndicatorSystem.TypingTeams[playerId] = team;

        if (Main.netMode == NetmodeID.Server)
        {
            Broadcast(playerId, isTyping, team, fromWho);

            string name = Main.player[playerId]?.name ?? playerId.ToString();
            //Log.Chat($"2: server received {name} is typing: {isTyping}");
        }
    }

    public static void Send(bool isTyping)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        ModPacket packet = ModContent.GetInstance<ChatPlus>().GetPacket();
        packet.Write((byte)PacketType.TypingIndicator);
        packet.Write(Main.myPlayer);
        packet.Write(isTyping);
        packet.Write(Main.LocalPlayer.team);

        if (Main.netMode == NetmodeID.MultiplayerClient)
            packet.Send();
        else
            packet.Send(-1, Main.myPlayer);
    }

    private static void Broadcast(int playerId, bool isTyping, int team, int ignore)
    {
        for (int toWho = 0; toWho < Main.maxPlayers; toWho++)
        {
            if (toWho == ignore) continue;
            if (!Main.player[toWho].active) continue;

            ModPacket packet = ModContent.GetInstance<ChatPlus>().GetPacket();
            packet.Write((byte)PacketType.TypingIndicator);
            packet.Write(playerId);
            packet.Write(isTyping);
            packet.Write(team); 
            packet.Send(toWho);
        }
    }

}
