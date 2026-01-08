using System;
using System.Collections.Generic;
using ChatPlus.Core.Chat;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChatPlus.Common.Compat.CustomTags;

public class CustomTagSystem : ModSystem
{
    public UserInterface ui;
    public static readonly Dictionary<string, CustomTagState> States = [];
    public static List<CustomTag> CustomTags = [];
    public static readonly Dictionary<string, Func<string, IEnumerable<(string insert, UIElement view)>>> Providers = [];

    public override void Load()
    {
        ui = new UserInterface();
        ui.SetState(null);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (!Main.drawingPlayerChat)
        {
            if (ui.CurrentState != null)
                ui.SetState(null);
            return;
        }

        string text = Main.chatText ?? string.Empty;
        int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);

        foreach (var kv in States)
        {
            string prefix = "[" + kv.Key;
            if (ChatTriggers.UnclosedTag(prefix).ShouldOpen(text, caret))
            {
                if (ui.CurrentState != kv.Value)
                {
                    ChatPlus.StateManager.CloseOthers(ui);
                    ui.SetState(kv.Value);
                    ui.CurrentState.Recalculate();
                }
                ui.Update(gameTime);
                return;
            }
        }

        if (ui.CurrentState != null)
            ui.SetState(null);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(l => l.Name.Equals("Vanilla: Death Text"));
        if (index == -1) return;

        layers.Insert(index, new LegacyGameInterfaceLayer(
            "ChatPlus: Custom Tag Panel",
            () =>
            {
                if (ui?.CurrentState != null)
                    ui.Draw(Main.spriteBatch, new GameTime());
                return true;
            },
            InterfaceScaleType.UI
        ));
    }
}
