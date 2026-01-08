using System.Collections.Generic;
using ChatPlus.Core.Chat;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Mentions;

[Autoload(Side = ModSide.Client)]
public class MentionSystem : ModSystem
{
    public UserInterface ui;
    public MentionState state;

    public override void Load()
    {
        ui = new UserInterface();
        state = new MentionState();
        ui.SetState(null);
    }

    public override void Unload()
    {
        ui = null;
        state = null;
    }

    public override void UpdateUI(GameTime gameTime)
    {
        // (we’ll add the suppression guard here in step 2)
        ChatPlus.StateManager.OpenStateByTriggers(
            gameTime,
            ui,
            state,
            ChatTriggers.UnclosedTag("[mention"),
            ChatTriggers.AtMentionWord()             
        );
    }


    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(l => l.Name.Equals("Vanilla: Death Text"));
        if (index == -1) index = layers.Count;
        layers.Insert(index, new LegacyGameInterfaceLayer(
            "ChatPlus: Mentions",
            () =>
            {
                if (ui?.CurrentState != null)
                    ui.Draw(Main.spriteBatch, new GameTime());
                return true;
            }, InterfaceScaleType.UI));
    }
}