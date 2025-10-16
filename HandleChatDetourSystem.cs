using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Chat
{
    /// <summary>
    /// This system handles detours of the chat to implement and modify some features related to the chat:
    /// 1. Handles TextSnippet inserting (such as ctrl-clicking an item into chat)
    /// </summary>
    public class HandleChatDetourSystem : ModSystem
    {
        public override void Load()
        {
            On_ChatManager.AddChatText += OverrideAddChatText;
        }

        public override void Unload()
        {
            On_ChatManager.AddChatText -= OverrideAddChatText;
        }

        
        // Because this method usually just adds to Main.chatText, we're replacing it entirely
        // This version will insert the text at the caretPos in Main.chatText.
        private bool OverrideAddChatText(On_ChatManager.orig_AddChatText orig, ReLogic.Graphics.DynamicSpriteFont font, string text, Vector2 baseScale)
        {
            int safeWidth = Main.screenWidth - 330;
            if (ChatManager.GetStringSize(font, Main.chatText + text, baseScale).X > safeWidth) return false;

            // Safety check, just in case, since inserting text could possibly insert at an out-of-bounds spot in the string
            HandleChatSystem.caretPos = Math.Clamp(HandleChatSystem.caretPos, 0, Main.chatText.Length);

            Main.chatText = Main.chatText.Insert(HandleChatSystem.caretPos, text);
            HandleChatSystem.caretPos += text.Length;
            return true;
        }
    }
}
