using System.Collections.Generic;
using ChatPlus.Core.Chat;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChatPlus.Core.Features.Colors
{
    [Autoload(Side = ModSide.Client)]
    public class ColorSystem : ModSystem
    {
        public UserInterface ui;
        public ColorState state;

        public override void Load()
        {
            ui = new UserInterface();
            state = new ColorState();
            ui.SetState(null);
        }
        public override void UpdateUI(GameTime gameTime)
        {
            ChatPlus.StateManager.OpenStateByTriggers(
                gameTime,
                ui,
                state,
                ChatTriggers.UnclosedTag("[c")
            );
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(l => l.Name.Equals("Vanilla: Death Text"));
            if (index == -1) return;

            layers.Insert(index, new LegacyGameInterfaceLayer(
                "ChatPlus: Colors Panel",
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
}
