using ChatPlus.Common.Configs;
using ChatPlus.Common.Debug;
using ChatPlus.Core.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.TypingIndicators;

internal class TypingIndicatorPlayer : ModPlayer
{
    private bool lastTyping;
    private Config.Privacy _lastMode = Config.Privacy.Everyone;

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        bool isTyping = Main.drawingPlayerChat && Main.chatText.Length >= 0;

        if (isTyping == lastTyping)
            return;

        lastTyping = isTyping;
        TypingIndicatorSystem.TypingPlayers[Main.myPlayer] = isTyping;

        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            //Log.Chat($"1: {Main.LocalPlayer.name} is typing: {isTyping}");
            TypingIndicatorNetHandler.Send(isTyping);
        }
    }

}

