using ChatPlus.Common.Configs;
using ChatPlus.Core.Misc;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.Stats.PlayerStats.Privacy;

internal static class PrivacyNetHandler
{
    private enum Op : byte
    {
        PrivacyUpdate = 1
    }

    public static void SendLocalPrivacy()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient) return;

        var privacy = Conf.C.StatsPrivacy;

        var packet = NewPacket(Op.PrivacyUpdate);
        packet.Write((byte)Main.myPlayer);
        packet.Write((byte)privacy);
        packet.Send();
    }

    public static void BroadcastSingle(int who, Config.StatsPrivacyMode privacy)
    {
        if (Main.netMode != NetmodeID.Server) return;

        PrivacyCache.Set(who, privacy);

        var packet = NewPacket(Op.PrivacyUpdate);
        packet.Write((byte)who);
        packet.Write((byte)privacy);
        packet.Send();
    }

    public static void ServerSyncTo(int toClient)
    {
        if (Main.netMode != NetmodeID.Server) return;

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            if (!Main.player[i].active) continue;

            var privacy = PrivacyCache.Get(i);

            var packet = NewPacket(Op.PrivacyUpdate);
            packet.Write((byte)i);
            packet.Write((byte)privacy);
            packet.Send(toClient);
        }
    }

    public static void Receive(BinaryReader reader, int fromWho)
    {
        var op = (Op)reader.ReadByte();
        if (op != Op.PrivacyUpdate) return;

        int playerId = reader.ReadByte();
        var privacy = (Config.StatsPrivacyMode)reader.ReadByte();

        PrivacyCache.Set(playerId, privacy);

        if (Main.netMode == NetmodeID.Server)
        {
            var packet = NewPacket(Op.PrivacyUpdate);
            packet.Write((byte)playerId);
            packet.Write((byte)privacy);
            packet.Send(-1, fromWho);
        }
    }

    private static ModPacket NewPacket(Op op)
    {
        var p = ModContent.GetInstance<ChatPlus>().GetPacket();
        p.Write((byte)PacketType.Privacy);
        p.Write((byte)op);
        return p;
    }
}
