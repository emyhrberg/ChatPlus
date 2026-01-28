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

        var mode = Conf.C.TypingIndicators;

        if (mode == Config.Privacy.NoOne)
        {
            if (_lastMode != Config.Privacy.NoOne || lastTyping)
            {
                lastTyping = false;
                TypingIndicatorSystem.TypingPlayers[Main.myPlayer] = false;

                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    //Log.Chat($"1: {Main.LocalPlayer.name} is typing: False");
                    TypingIndicatorNetHandler.Send(false);
                }
            }

            _lastMode = mode;
            return;
        }

        _lastMode = mode;

        bool isTyping = Main.drawingPlayerChat && Main.chatText.Length >= 0;

        if (isTyping != lastTyping)
        {
            lastTyping = isTyping;
            TypingIndicatorSystem.TypingPlayers[Main.myPlayer] = isTyping;

            Log.Chat($"1: {Main.LocalPlayer.name} is typing: {isTyping}");
            TypingIndicatorNetHandler.Send(isTyping);
        }
    }
}

