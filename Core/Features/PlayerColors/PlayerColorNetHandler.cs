using ChatPlus.Core.Features.Mentions;
using ChatPlus.Core.Misc;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.PlayerColors;

internal static class PlayerColorNetHandler
{
    private enum Msg : byte
    {
        Hello = 1,
        SyncSingle = 2,
        SyncAll = 3
    }

    public static void ClientHello(int who, string hex)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient) return;
        if (string.IsNullOrWhiteSpace(hex)) hex = "FFFFFF";

        var p = NewPacket(Msg.Hello);
        p.Write((byte)who);
        p.Write(hex);
        p.Send();
    }

    public static void Receive(BinaryReader reader, int fromWho)
    {
        var msg = (Msg)reader.ReadByte();

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            switch (msg)
            {
                case Msg.SyncSingle:
                    {
                        byte who = reader.ReadByte();
                        string hex = reader.ReadString();

                        PlayerColorSystem.PlayerColors[who] = SanHex(hex);

                        var name = Main.player[who]?.name;
                        if (!string.IsNullOrWhiteSpace(name))
                            MentionSnippet.InvalidateCachesFor(name);
                        else
                            MentionSnippet.ClearAllCaches();

                        break;
                    }

                case Msg.SyncAll:
                    {
                        PlayerColorSystem.PlayerColors.Clear();
                        int count = reader.ReadByte();
                        for (int i = 0; i < count; i++)
                        {
                            byte who = reader.ReadByte();
                            string hex = reader.ReadString();
                            PlayerColorSystem.PlayerColors[who] = SanHex(hex);
                        }
                        MentionSnippet.ClearAllCaches();
                        break;
                    }
            }
            return;
        }

        if (Main.netMode == NetmodeID.Server)
        {
            switch (msg)
            {
                case Msg.Hello:
                    {
                        byte who = reader.ReadByte();
                        string requested = SanHex(reader.ReadString());
                        string assigned = requested;

                        if (assigned == "FFFFFF")
                            PlayerColorSystem.PlayerColors.Remove(who);
                        else
                            PlayerColorSystem.PlayerColors[who] = assigned;

                        var all = NewPacket(Msg.SyncAll);
                        var map = PlayerColorSystem.PlayerColors;
                        all.Write((byte)map.Count);
                        foreach (var kv in map)
                        {
                            all.Write((byte)kv.Key);
                            all.Write(SanHex(kv.Value));
                        }
                        all.Send(toClient: who);

                        var one = NewPacket(Msg.SyncSingle);
                        one.Write(who);
                        one.Write(assigned);
                        one.Send();

                        break;
                    }
            }
        }
    }

    private static ModPacket NewPacket(Msg msg)
    {
        var p = ModContent.GetInstance<ChatPlus>().GetPacket();
        p.Write((byte)PacketType.PlayerColor);
        p.Write((byte)msg);
        return p;
    }

    private static string SanHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return "FFFFFF";
        hex = hex.Trim().TrimStart('#');
        if (hex.Length != 6) return "FFFFFF";

        for (int i = 0; i < 6; i++)
        {
            char c = hex[i];
            if (!(c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F'))
                return "FFFFFF";
        }
        return hex;
    }
}
