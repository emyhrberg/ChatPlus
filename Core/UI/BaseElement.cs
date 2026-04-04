using System.Reflection;
using ChatPlus.Core.Features.Uploads;
using ChatPlus.Core.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader.UI;
using Terraria.UI;
using static ChatPlus.Common.Configs.ClientConfig;

namespace ChatPlus.Core.UI;

/// <summary>
/// An element that can be navigated in a <see cref="BasePanel<TData>"/>.
/// </summary>
public abstract class BaseElement<TData> : UIElement
{
    private bool isSelected;
    public bool SetSelected(bool value) => isSelected = value;

    public TData Data { get; }

    protected BasePanel<TData> GetParentPanel()
    {
        UIElement parent = Parent;
        while (parent != null && parent is not BasePanel<TData>)
        {
            parent = parent.Parent;
        }
        return parent as BasePanel<TData>;
    }

    protected Viewmode GetViewmode()
    {
        var parent = GetParentPanel();
        return parent != null ? parent.CurrentViewMode : Viewmode.List;  
    }
    protected bool IsGridSwitchSuppressed()
    {
        var parent = GetParentPanel();
        return parent?.IsGridSwitchSuppressed == true;
    }

    protected BaseElement(TData data)
    {
        Height.Set(30, 0);
        Width.Set(0, 1);
        Data = data;
    }

    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);


        bool leftShiftDown = Main.keyState.IsKeyDown(Keys.LeftShift);
        if (leftShiftDown) return;

        var panel = GetParentPanel();
        if (panel == null) return;                        

        int index = panel.items.IndexOf(this);
        if (index >= 0)
        {
            panel.SetSelectedIndex(index);
            panel.InsertSelectedTag();
        }
    }

    // Ask the parent panel for this element’s description.
    private string TryGetDescription()
    {
        var panel = GetParentPanel();
        if (panel == null) return null;

        // BasePanel<TData> has a protected GetDescription(TData) — call it via reflection.
        var mi = panel.GetType().GetMethod(
            "GetDescription",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );
        if (mi == null) return null;

        try
        {
            return mi.Invoke(panel, new object[] { Data }) as string;
        }
        catch
        {
            return null;
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (IsMouseHovering)
        {
            var panel = GetParentPanel();
            if (panel != null)
            {
                int index = panel.items.IndexOf(this);
                if (index >= 0 && panel.CurrentIndex != index)
                    panel.SetSelectedIndex(index);
            }
        }
    }

    public override void Draw(SpriteBatch sb)
    {
        // selection visuals (unchanged) ...
        if (isSelected)
        {
            DrawHelper.DrawSlices(sb, ele: this);
            DrawHelper.DrawFill(sb, ele: this);
        }
        else
        {
            Rectangle r = GetDimensions().ToRectangle();
            DrawHelper.DrawPixelatedBorder(sb, r, Color.Black * 0.75f, 2, 1);
        }

        if (this is not UploadElement)
            base.Draw(sb);

        // list/grid draw (unchanged) ...
        bool forceGrid = IsGridSwitchSuppressed();
        if (!forceGrid && GetViewmode() == Viewmode.List)
            DrawListElement(sb);
        else
            DrawGridElement(sb);

        // NEW: hover tooltip with the element’s description
        if (IsMouseHovering)
        {
            string desc = TryGetDescription();
            if (!string.IsNullOrWhiteSpace(desc))
                UICommon.TooltipMouseText(desc);

            Main.LocalPlayer.mouseInterface = true;
        }
    }

    // subclasses must provide these
    protected abstract void DrawListElement(SpriteBatch sb);
    protected abstract void DrawGridElement(SpriteBatch sb);
}


