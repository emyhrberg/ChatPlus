using System;
using System.Linq;
using ChatPlus.Common.Configs;
using ChatPlus.Core.Misc;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ChatPlus.Core.Chat.ChatButtons.Shared;

internal abstract class BaseChatButton : UIElement
{
    protected ChatButtonType Type { get; }
    protected abstract UserInterface UI { get; }
    protected abstract UIState State { get; }
    protected abstract Action<bool> SetOpenedByButton { get; }
    protected virtual Color ColorWhenEnabled => Color.White;
    protected virtual Color ColorWhenDisabled => new Color(180, 180, 180);
    protected BaseChatButton(ChatButtonType type)
    {
        Type = type;
        Width.Set(24, 0f);
        Height.Set(24, 0f);
    }
    protected DraggablePanel GetPanel()
    {
        return ChatPlus.StateManager?.GetActivePanel();
    }

    public override void LeftClick(UIMouseEvent evt)
    {
        if (UI.CurrentState == null)
        {
            ChatPlus.StateManager.ResetAllWasOpenedByButton();
            ChatPlus.StateManager.CloseOthers(UI);

            SetOpenedByButton(true);

            var dims = GetDimensions();
            var buttonPos = dims.Position();
            int buttonSize = (int)dims.Width;

            DraggablePanel.RequestNextSnap(buttonPos, buttonSize);

            UI.SetState(State);
            return;
        }

        SetOpenedByButton(false);
        UI.SetState(null);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        //Log.Debug(Type == ChatButtonType.Viewmode);
        // Optional safety: if enabled but somehow detached, re-attach
        if (Type == ChatButtonType.Viewmode && (Conf.C?.ShowViewmodeButton ?? true))
        {
            //Log.Debug("ON");
            var sys = ModContent.GetInstance<ChatButtonsSystem>();
            if (Parent != sys.state)
                sys.state.Append(this);
        }

        int gap = (Type == ChatButtonType.Config || Type == ChatButtonType.Viewmode) ? 2 : 2;
        var pos = ChatButtonLayout.ComputeTopLeft(Type, size: 24, gap: gap);
        if (Math.Abs(Left.Pixels - pos.X) > 0.5f || Math.Abs(Top.Pixels - pos.Y) > 0.5f)
        {
            Left.Set(pos.X, 0f);
            Top.Set(pos.Y, 0f);
            Recalculate();
        }
    }


    public override void Draw(SpriteBatch sb)
    {
        if (IsMouseHovering)
        {
            Main.LocalPlayer.mouseInterface = true;

            if (Type == ChatButtonType.Viewmode)
            {
                var panel = GetPanel();
                if (panel != null)
                {
                    var panelType = panel.GetType();
                    var current = ChatButtonLayout.GetViewmodeFor(panelType);
                    UICommon.TooltipMouseText("View: " + current.ToString());

                    // Draw border
                    var r2 = GetDimensions().ToRectangle();
                    r2.Inflate(2, 2);
                    DrawHelper.DrawPixelatedBorder(sb, r2, Color.Gray, 2, 2);
                }
            }
            else
            {
                string tt = Type.ToString();

                if (Type == ChatButtonType.PlayerIcons) tt = "Player Icons";
                if (Type == ChatButtonType.ModIcons) tt = "Mod Icons";

                UICommon.TooltipMouseText(tt);

                // Draw border
                var r = GetDimensions().ToRectangle();
                r.Inflate(2, 2);
                DrawHelper.DrawPixelatedBorder(sb, r, Color.Gray, 2, 2);
            }
        }

        var dims = GetDimensions();
        bool forceNormal = IsMouseHovering || UI.CurrentState == State;
        ChatButtonRenderer.Draw(sb, Type, dims.Position(), 24, grayscale: !forceNormal);
    }

    public override bool ContainsPoint(Vector2 point)
    {
        return base.ContainsPoint(point);
    }
}
