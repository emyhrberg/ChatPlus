﻿using System;
using System.Reflection;
using AdvancedChatFeatures.ColorHandler;
using AdvancedChatFeatures.Commands;
using AdvancedChatFeatures.Emojis;
using AdvancedChatFeatures.Glyphs;
using AdvancedChatFeatures.Helpers;
using AdvancedChatFeatures.ItemWindow;
using AdvancedChatFeatures.UI;
using AdvancedChatFeatures.Uploads;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Core.Platforms;
using Terraria;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader;

namespace AdvancedChatFeatures.Common.Hooks
{
    /// <summary>
    /// This system does IL edits the chat to implement and modify some features related to the chat:
    /// 1. Skips chat lines from scrolling when pressing up and down arrow keys.
    /// </summary>
    public class HandleChatILHook : ModSystem
    {
        public override void Load()
        {
            IL_Main.DoUpdate_HandleChat += ModifyToggleChat;
        }

        public override void Unload()
        {
            IL_Main.DoUpdate_HandleChat -= ModifyToggleChat;
        }

        /// <summary>
        /// Does 2 things:
        /// 1. Skip scrolling with up/down arrow keys when any of my states are open
        /// 2. Skip closing the chat when escape is pressed when any of my states are open
        /// </summary>
        private void ModifyToggleChat(ILContext il)
        {
            try
            {
                // Match to linesOffset setter
                var c = new ILCursor(il);
                if (c.TryGotoNext(
                        i => i.MatchLdsfld(typeof(Main), nameof(Main.chatMonitor)),
                        i => i.MatchLdloc0(),
                        i => i.MatchCallvirt(typeof(IChatMonitor), nameof(IChatMonitor.Offset))))
                {
                    c.Index += 2;

                    // Set linesOffset = 0 if any state is active.
                    // This effectively stops lines scrolling if any state is active.
                    c.EmitDelegate<Func<int, int>>(v => IsAnyStateActive() ? 0 : v);
                }
                else
                {
                    Log.Error("Failed to patch linesOffset in HandleChat.");
                }

                c.Index = 0;

                //        if (c.TryGotoNext(
                //    i => i.MatchLdsflda(typeof(Main), nameof(Main.keyState)),
                //    i => i.MatchLdcI4((int)Keys.Escape),
                //    i => i.MatchCall(typeof(KeyboardState), nameof(KeyboardState.IsKeyDown)),
                //    i => i.MatchBrfalse(out var skipEscBlock) // capture the original target label
                //))
                //        {
                //            int oldIndex = c.Index;
                //            // oldIndex should be at the *start* of the ESC block (the ldsflda Main.keyState)
                //            if (c.TryGotoNext(i => i.MatchStsfld<Main>(nameof(Main.drawingPlayerChat))))
                //            {
                //                // mark jump target *after* the ESC-close write
                //                c.Index++;
                //                ILLabel afterEscBlock = il.DefineLabel();
                //                c.MarkLabel(afterEscBlock);

                //                // return to the start of the ESC block
                //                c.Index = oldIndex;

                //                var mi = typeof(HandleChatILHook).GetMethod(nameof(IsAnyStateActive));

                //                c.EmitCall(mi);          // push bool result
                //                c.EmitBrtrue(afterEscBlock);
                //            }
                //        }
            }

            catch (Exception ex)
            {
                Log.Error($"HandleChatILHook failed: {ex}");
            }
            MonoModHooks.DumpIL(Mod, il);
        }

        public static bool IsAnyStateActive()
        {
            var cmdSys = ModContent.GetInstance<CommandSystem>();
            var colorSys = ModContent.GetInstance<ColorSystem>();
            var emojiSys = ModContent.GetInstance<EmojiSystem>();
            var glyphSys = ModContent.GetInstance<GlyphSystem>();
            var itemSys = ModContent.GetInstance<ItemSystem>();
            var uploadSys = ModContent.GetInstance<UploadSystem>();

            if (cmdSys.ui.CurrentState != null || 
                colorSys.ui.CurrentState != null || 
                emojiSys.ui.CurrentState != null || 
                glyphSys.ui.CurrentState != null || 
                itemSys.ui.CurrentState != null || 
                uploadSys.ui.CurrentState != null)
            {
                return true;
            }

            return false;
        }
    }
}

