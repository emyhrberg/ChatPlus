using System;
using System.Collections.Generic;
using System.Linq;
using ChatPlus.Common.Compat.CustomTags;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static ChatPlus.Common.Configs.ClientConfig;

namespace ChatPlus.Core.UI;

public abstract class DraggablePanel : UIPanel
{
    public virtual void SetViewmode(Viewmode vm) { }

    private bool dragging;
    private bool pendingDrag; // mouse down occurred but threshold not yet exceeded
    private Vector2 dragOffset;
    private const float dragThreshold = 3f;
    private Vector2 mouseDownPos;
    public bool IsDragging => dragging;
    public static bool AnyHovering;

    public DraggablePanel ConnectedPanel { get; set; }
    protected virtual float SharedYOffset => 0f;

    public override void OnActivate()
    {
        base.OnActivate();
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        AnyHovering = true;
    }

    public override void MouseOut(UIMouseEvent evt)
    {
        base.MouseOut(evt);
        AnyHovering = false;
    }

    #region Snap
    private static bool hasPendingSnap;
    private static Vector2 pendingSnapPos;
    private static int pendingSnapSize;

    public static void RequestNextSnap(Vector2 buttonPos, int buttonSize)
    {
        pendingSnapPos = buttonPos;
        pendingSnapSize = buttonSize;
        hasPendingSnap = true;
    }

    public static bool TryConsumeNextSnap(out Vector2 buttonPos, out int buttonSize)
    {
        if (hasPendingSnap)
        {
            hasPendingSnap = false;
            buttonPos = pendingSnapPos;
            buttonSize = pendingSnapSize;
            return true;
        }

        buttonPos = default;
        buttonSize = 0;
        return false;
    }

    public void SnapRightAlignedTo(Vector2 buttonPos, int buttonSize)
    {
        float panelWidth = Width.Pixels;
        if (panelWidth <= 0f)
        {
            Recalculate();
            panelWidth = GetOuterDimensions().Width;
        }

        float rightEdge = buttonPos.X + buttonSize;
        float newX = rightEdge - panelWidth;

        Left.Set((float)Math.Round(newX), 0f);
        Recalculate();
    }
    #endregion

    public override void Update(GameTime gameTime)
    {
        if (IsMouseHovering)
            Main.LocalPlayer.mouseInterface = true;

        // Determine if we should enter dragging after surpassing threshold
        if (pendingDrag && !dragging)
        {
            float moved = Vector2.Distance(new Vector2(Main.mouseX, Main.mouseY), mouseDownPos);
            if (moved > dragThreshold)
                dragging = true;
        }

        if (!dragging) return;

        var r = GetDimensions().ToRectangle();
        float oldX = Left.Pixels;
        float targetX = Main.mouseX - dragOffset.X;
        float rawDx = targetX - oldX;

        float dxMin = -r.Left;
        float dxMax = Main.screenWidth - r.Right;

        if (ConnectedPanel != null)
        {
            var other = ConnectedPanel.GetDimensions().ToRectangle();
            dxMin = Math.Max(dxMin, -other.Left);
            dxMax = Math.Min(dxMax, Main.screenWidth - other.Right);
        }

        float dx = MathHelper.Clamp(rawDx, dxMin, dxMax);

        Left.Set(oldX + dx, 0f);
        Recalculate();

        if (ConnectedPanel != null)
        {
            ConnectedPanel.Left.Set(ConnectedPanel.Left.Pixels + dx, 0f);
            ConnectedPanel.Recalculate();
        }
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        bool leftShiftDown = Main.keyState.IsKeyDown(Keys.LeftShift);

        if (leftShiftDown) return;
        if (IsAnyScrollbarHovering()) return;

        mouseDownPos = evt.MousePosition;
        base.LeftMouseDown(evt);
        dragging = false; // will become true after threshold
        pendingDrag = true;
        dragOffset = evt.MousePosition - new Vector2(Left.Pixels, Top.Pixels);
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        base.LeftMouseUp(evt);
        dragging = false;
        pendingDrag = false;
    }

    public static bool IsAnyScrollbarHovering()
    {
        if (CustomTagSystem.States != null && CustomTagSystem.States.Count > 0)
        {
            foreach (BaseState<CustomTag> state in CustomTagSystem.States.Values)
            {
                if (state.Panel.scrollbar.IsMouseHovering == true) return true;
            }
        }

        return
            ChatPlus.StateManager.CommandSystem?.state?.Panel?.scrollbar?.IsMouseHovering == true ||
            ChatPlus.StateManager.ColorSystem?.state?.Panel?.scrollbar?.IsMouseHovering == true ||
            ChatPlus.StateManager.EmojiSystem?.state?.Panel?.scrollbar?.IsMouseHovering == true ||
            ChatPlus.StateManager.GlyphSystem?.state?.Panel?.scrollbar?.IsMouseHovering == true ||
            ChatPlus.StateManager.ItemSystem?.state?.Panel?.scrollbar?.IsMouseHovering == true ||
            ChatPlus.StateManager.ModIconSystem?.state?.Panel?.scrollbar?.IsMouseHovering == true ||
            ChatPlus.StateManager.MentionSystem?.state?.Panel?.scrollbar?.IsMouseHovering == true ||
            ChatPlus.StateManager.PlayerIconSystem?.state?.Panel?.scrollbar?.IsMouseHovering == true ||
            ChatPlus.StateManager.UploadSystem?.state?.Panel?.scrollbar?.IsMouseHovering == true;
    }
}