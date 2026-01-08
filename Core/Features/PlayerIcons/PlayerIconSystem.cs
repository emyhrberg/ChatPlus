using System.Collections.Generic;
using ChatPlus.Core.Chat;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChatPlus.Core.Features.PlayerIcons
;

[Autoload(Side = ModSide.Client)]
public class PlayerIconSystem : ModSystem
{
    public UserInterface ui;
    public PlayerIconState state;

    public override void Load()
    {
        ui = new UserInterface();
        state = new PlayerIconState();
        ui.SetState(null);
    }

    public override void Unload()
    {
        ui = new UserInterface();
        state = new PlayerIconState();
        ui.SetState(null);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        ChatPlus.StateManager.OpenStateByTriggers(
            gameTime,
            ui,
            state,
            ChatTriggers.UnclosedTag("[p")
        );
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(l => l.Name.Equals("Vanilla: Death Text"));
        if (index == -1)
            index = layers.Count;

        layers.Insert(index, new LegacyGameInterfaceLayer(
            "ChatPlus: PlayerIcons",
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
