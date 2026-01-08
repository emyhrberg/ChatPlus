using System.IO;
using ChatPlus.Core.Misc;
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
        ModPacket packet = ModContent.GetInstance<ChatPlus>().GetPacket();
        packet.Write((byte)PacketType.TypingIndicator);
        packet.Write(playerId);
        packet.Write(isTyping);
        packet.Send(-1, ignore);
    }
}
