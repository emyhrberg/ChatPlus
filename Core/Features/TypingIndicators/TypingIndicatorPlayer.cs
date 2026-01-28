using ChatPlus.Common.Configs;
using ChatPlus.Core.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.TypingIndicators;
internal class TypingIndicatorPlayer : ModPlayer
{
    private bool lastTyping;

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer) return;
        if (Conf.C.TypingIndicators == Config.Privacy.NoOne) return;

        bool isTyping =
            //Main.hasFocus && 
            //Main.instance.IsActive && 
            //!Main.blockInput &&
            Main.drawingPlayerChat
            && Main.chatText.Length >= 0;

        if (isTyping != lastTyping)
        {
            lastTyping = isTyping;
            TypingIndicatorSystem.TypingPlayers[Main.myPlayer] = isTyping;
            SendTypingState(isTyping);
        }
    }

    public static void SendTypingState(bool isTyping)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        ModPacket packet = ModContent.GetInstance<ChatPlus>().GetPacket();
        packet.Write((byte)PacketType.TypingIndicator);
        packet.Write(Main.myPlayer);
        packet.Write(isTyping);

        if (Main.netMode == NetmodeID.MultiplayerClient)
            packet.Send();
        else if (Main.netMode == NetmodeID.Server)
            packet.Send(-1, Main.myPlayer);
    }
}
