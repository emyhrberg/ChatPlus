using System.Collections.Generic;
using ChatPlus.Core.Chat;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChatPlus.Core.Features.Uploads;

[Autoload(Side = ModSide.Client)]
public class UploadSystem : ModSystem
{
    public UserInterface ui;
    public UploadState state;

    public override void Load()
    {
        ui = new UserInterface();
        state = new UploadState();
        ui.SetState(null);
    }

    public override void Unload()
    {
        ui = new UserInterface();
        state = new UploadState();
        ui.SetState(null);
    }
    public static bool OpenedFromHash { get; private set; }
    public override void UpdateUI(GameTime gameTime)
    {
        string text = Main.chatText ?? string.Empty;
        int caret = text.Length;

        var unclosedTag = ChatTriggers.UnclosedTag("[u");
        var hashWord = ChatTriggers.CurrentWordStartsWith('#');

        // Hash mode only if '#' is active and a [u tag is NOT active
        OpenedFromHash = hashWord.ShouldOpen(text, caret) && !unclosedTag.ShouldOpen(text, caret);

        ChatPlus.StateManager.OpenStateByTriggers(
            gameTime,
            ui,
            state,
            unclosedTag,
            hashWord
        );
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(l => l.Name.Equals("Vanilla: Death Text"));
        if (index == -1) return;

        layers.Insert(index, new LegacyGameInterfaceLayer(
            "ChatPlus: Uploads Panel",
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
