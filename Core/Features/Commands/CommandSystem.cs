using System.Collections.Generic;
using ChatPlus.Core.Chat;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChatPlus.Core.Features.Commands;

[Autoload(Side = ModSide.Client)]
public class CommandSystem : ModSystem
{
    public UserInterface ui;
    public CommandState state;

    public override void Load()
    {
        ui = new UserInterface();
        state = new CommandState();
        ui.SetState(null); // start hidden
    }
    public override void UpdateUI(GameTime gameTime)
    {
        ChatPlus.StateManager.OpenStateByTriggers(
            gameTime,
            ui,
            state,
            ChatTriggers.CurrentWordStartsWith('/')
        );
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(l => l.Name.Equals("Vanilla: Death Text"));
        if (index == -1) return;

        layers.Insert(index, new LegacyGameInterfaceLayer(
            "ChatPlus: Commands Panel",
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