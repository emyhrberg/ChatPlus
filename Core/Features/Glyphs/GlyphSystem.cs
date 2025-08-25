﻿using System.Collections.Generic;
using ChatPlus.GlyphHandler;
using ChatPlus.Helpers;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChatPlus.Core.Features.Glyphs
{
    [Autoload(Side = ModSide.Client)]
    public class GlyphSystem : ModSystem
    {
        public UserInterface ui;
        public GlyphState glyphState;

        public override void Load()
        {
            ui = new UserInterface();
            glyphState = new GlyphState();
            ui.SetState(null);
        }

        public override void Unload()
        {
            ui = new UserInterface();
            glyphState = new GlyphState();
            ui.SetState(null);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            StateHandler.OpenStateIfPrefixMatches(gameTime, ui, glyphState, "[g");
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(l => l.Name.Equals("Vanilla: Death Text"));
            if (index == -1) return;

            layers.Insert(index, new LegacyGameInterfaceLayer(
                "ChatPlus: GlyphSystem",
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
