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

        TypingIndicatorSystem.TypingPlayers[playerId] = isTyping;

        if (Main.netMode == NetmodeID.Server)
        {
            string name = Main.player[playerId]?.name ?? playerId.ToString();
            //Log.Chat($"2: server received {name} is typing: {isTyping}");

            Broadcast(playerId, isTyping, fromWho);
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

        if (Main.netMode == NetmodeID.MultiplayerClient)
            packet.Send();
        else
            packet.Send(-1, Main.myPlayer);
    }


    private static void Broadcast(int playerId, bool isTyping, int ignore)
    {
        string name = Main.player[playerId]?.name ?? playerId.ToString();
        //Log.Chat($"3: server broadcast {name} is typing: {isTyping}");

        for (int toWho = 0; toWho < Main.maxPlayers; toWho++)
        {
            if (toWho == ignore) continue;
            if (!Main.player[toWho].active) continue;

            ModPacket packet = ModContent.GetInstance<ChatPlus>().GetPacket();
            packet.Write((byte)PacketType.TypingIndicator);
            packet.Write(playerId);
            packet.Write(isTyping);
            packet.Send(toWho);
        }
    }


}
