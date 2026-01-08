using ChatPlus.Core.Chat;
using ChatPlus.Core.Features.Commands;
using ChatPlus.Core.Features.Emojis;
using ChatPlus.Core.Features.Scrollbar;
using ChatPlus.Core.Features.Uploads;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ChatPlus.Common
{
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind CommandKB;
        public static ModKeybind EmojiKB;
        public static ModKeybind UploadsKB;

        public override void Load()
        {
            CommandKB = KeybindLoader.RegisterKeybind(Mod, "Open Command Window", Keys.C);
            EmojiKB = KeybindLoader.RegisterKeybind(Mod, "Open Emoji Window", Keys.Y);
            UploadsKB = KeybindLoader.RegisterKeybind(Mod, "Open Upload Window", Keys.U);
        }
    }

    public class KeybindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet t)
        {
            static void Open<TSystem>(ModKeybind keybind, TSystem sys, string prefix) where TSystem : ModSystem
            {
                if (!keybind.JustPressed)
                    return;

                // If chat is open, keybind is ignored
                if (Main.drawingPlayerChat)
                    return;

                // Force open chat and input
                Main.drawingPlayerChat = true;
                PlayerInput.WritingText = true;
                Main.chatRelease = false;

                // Add the system prefix.
                // The system will handle opening the chatScrollState by checking for the prefix.
                Main.chatText = prefix;

                // Set caret to last
                HandleChatSystem.SetCaretPos(Main.chatText.Length);
            }

            Open(KeybindSystem.CommandKB, ModContent.GetInstance<CommandSystem>(), "/");
            Open(KeybindSystem.EmojiKB, ModContent.GetInstance<EmojiSystem>(), "[e");
            Open(KeybindSystem.UploadsKB, ModContent.GetInstance<UploadSystem>(), "#");
        }
    }
}