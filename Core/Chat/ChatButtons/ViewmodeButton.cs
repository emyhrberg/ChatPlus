using System;
using ChatPlus.Core.Chat.ChatButtons.Shared;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using static ChatPlus.Common.Configs.ClientConfig;

namespace ChatPlus.Core.Chat.ChatButtons;

internal class ViewmodeButton : BaseChatButton
{
    public ViewmodeButton() : base(ChatButtonType.Viewmode) { }

    // This button toggles the view mode instead of toggling a UI state
    protected override UserInterface UI => ModContent.GetInstance<ChatButtonsSystem>().ui; // unused
    protected override UIState State => ModContent.GetInstance<ChatButtonsSystem>().state; // unused
    protected override Action<bool> SetOpenedByButton => _ => { };
    public override void LeftClick(UIMouseEvent evt)
    {
        DraggablePanel panel = GetPanel();
        if (panel == null) return;

        var panelType = panel.GetType();
        var current = ChatButtonLayout.GetViewmodeFor(panelType);
        Viewmode next = current == Viewmode.List ? Viewmode.Grid : Viewmode.List;

        // Flip the dictionary
        ChatButtonLayout.DefaultViewmodes[panelType] = next;

        // Apply the view mode
        panel.SetViewmode(next);
    }

    //public override void Draw(SpriteBatch sb)
    //{
    //    base.Draw(sb);
    //}
}
