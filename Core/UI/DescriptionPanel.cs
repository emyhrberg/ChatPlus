using System;
using ChatPlus.Common.Configs;
using ChatPlus.Core.Features.Colors;
using ChatPlus.Core.Features.Commands;
using ChatPlus.Core.Features.Emojis;
using ChatPlus.Core.Features.Glyphs;
using ChatPlus.Core.Features.ModIcons;
using ChatPlus.Core.Features.PlayerIcons;
using ChatPlus.Core.Features.Uploads;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ChatPlus.Core.UI;

public class DescriptionPanel<TData> : DraggablePanel
{
    private readonly UIText text;
    public UIText GetText() => text;
    private bool initialized;

    protected override float SharedYOffset
    {
        get
        {
            const float pad = 0f;
            float visible = Conf.C?.AutocompleteItemsVisible ?? 10f;
            return -(visible * 30f) - pad; // sit above the base panel
        }
    }
    public DescriptionPanel(string initialText = null)
    {
        // Size
        Width.Set(300, 0);
        Height.Set(60, 0);
        BackgroundColor = new Color(33, 43, 79) * 1.0f;
        VAlign = 1f;
        Left.Set(190, 0);

        // Provide immediate default header so first open frame isn't blank.
        if (string.IsNullOrEmpty(initialText))
        {
            if (typeof(TData) == typeof(Command)) initialText = "[c/FFF014:Commands]";
            else if (typeof(TData) == typeof(ColorEntry)) initialText = "[c/FFF014:Colors]";
            else if (typeof(TData) == typeof(Emoji)) initialText = "[c/FFF014:Emojis]";
            else if (typeof(TData) == typeof(Glyph)) initialText = "[c/FFF014:Glyphs]";
            else if (typeof(TData) == typeof(Features.Items.ItemEntry)) initialText = "[c/FFF014:Items]";
            else if (typeof(TData) == typeof(ModIcon)) initialText = "[c/FFF014:Mods]";
            else if (typeof(TData) == typeof(PlayerIcon)) initialText = "[c/FFF014:Players]";
            else if (typeof(TData) == typeof(Upload)) initialText = "Click here to upload images \nRight click to open folder";
            else initialText = string.Empty;
        }

        // Text element
        text = new(initialText, 0.9f, false);

        Append(text);
    }

    public override void LeftClick(UIMouseEvent evt)
    {
        if (IsDragging)
            return;

        // ShowUploadButton
        if (typeof(TData) == typeof(Upload) && ConnectedPanel is UploadPanel up)
        {
            up.UploadImage();
            up.PopulatePanel();
            return;
        }

        //// Mods: open the “view more” page
        //if (typeof(TData) == typeof(ModIcon) && ConnectedPanel is ModIconPanel mp)
        //{
        //    mp.OpenModInfoForSelectedMod();
        //    return;
        //}

        //// Players: open the “view more” page
        //if (typeof(TData) == typeof(PlayerIcon) && ConnectedPanel is PlayerIconPanel ph)
        //{
        //    ph.OpenPlayerInfoForSelected();
        //    return;
        //}
    }

    public override void RightClick(UIMouseEvent evt)
    {
        if (IsDragging)
            return;

        // ShowUploadButton
        if (typeof(TData) == typeof(Upload) && ConnectedPanel is UploadPanel up)
        {
            up.OpenUploadsFolder();
            return;
        }
    }

    public override void OnActivate()
    {
        base.OnActivate();

        // a silly fix to avoid drawing at the wrong position the first frame
        initialized = false;
        initializeFrameCounter = 1; // when it reaches 0 -> initialize
    }
    private int initializeFrameCounter;

    public void SetText(string rawText)
    {
        var font = FontAssets.MouseText.Value;
        float maxWidth = Width.Pixels;

        // Special case: uploads
        if (typeof(TData) == typeof(Features.Uploads.Upload))
        {
            string t = "Click here to upload images \nRight click to open folder";
            text.SetText(t);
            text.VAlign = 0f;
            Height.Set(62, 0);
            base.Recalculate();
            return;
        }

        // If caller already gave us newlines (e.g., "Name\nClick to view more"), just use them.
        if (!string.IsNullOrEmpty(rawText) && rawText.IndexOf('\n') >= 0)
        {
            text.SetText(rawText);

            // Decide height from the number of lines
            int lines = 1;
            for (int i = 0; i < rawText.Length; i++)
                if (rawText[i] == '\n') lines++;

            text.VAlign = lines >= 2 ? 0f : 0.5f;
            int h = lines switch { 1 => 38, 2 => 62, _ => 90 };
            Height.Set(h, 0);
            Recalculate();
            return;
        }

        // Single-line input without explicit newline: fit or split into two lines.
        if (font.MeasureString(rawText).X <= maxWidth || rawText.Contains('['))
        {
            text.SetText(rawText);
            text.VAlign = 0.5f;
            Height.Set(38, 0);
            Recalculate();
            return;
        }

        // Split at the last space that still fits on the first line (closest to the right edge).
        int bestSplit = -1;
        for (int i = 0; i < rawText.Length; i++)
        {
            if (rawText[i] == ' ')
            {
                float w = font.MeasureString(rawText[..i]).X;
                if (w <= maxWidth) bestSplit = i; else break;
            }
        }
        if (bestSplit == -1) bestSplit = rawText.Length / 2; // naive fallback

        string first = rawText[..bestSplit].TrimEnd();
        string second = rawText[(bestSplit + 1)..].TrimStart();

        // Always force two lines
        text.SetText(first + "\n" + second);
        text.VAlign = 0f;
        Height.Set(62, 0);
        Recalculate();
    }

    public override void Draw(SpriteBatch sb)
    {
        if (!initialized)
            return; // skip until first valid position is set

        if (typeof(TData) == typeof(Features.Uploads.Upload))
        {
            // Lock panel height
            MinHeight.Set(62, 0);
            MaxHeight.Set(62, 0);
            Height.Set(62, 0);

            text.VAlign = 0f;           
            text.Top.Set(0, 0);         
            text.Recalculate();          
        }

        base.Draw(sb);
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (ConnectedPanel == null)
        {
            return;
        }

        // Always use absolute pixel positioning when attached
        HAlign = 0f;
        VAlign = 0f;

        var baseRect = ConnectedPanel.GetDimensions().ToRectangle();
        float gap = 0;

        float desiredLeft = baseRect.Left;
        float desiredTop = baseRect.Top - Height.Pixels - gap;

        if (desiredLeft < 0f)
        {
            desiredLeft = 0f;
        }
        if (desiredLeft + Width.Pixels > Main.screenWidth)
        {
            desiredLeft = Main.screenWidth - Width.Pixels;
        }
        if (desiredTop < 0f)
        {
            desiredTop = 0f;
        }

        if (Math.Abs(Left.Pixels - desiredLeft) > 0.5f || Math.Abs(Top.Pixels - desiredTop) > 0.5f)
        {
            Left.Set(desiredLeft, 0f);
            Top.Set(desiredTop, 0f);
            Recalculate();
        }

        if (initializeFrameCounter > 0)
        {
            initializeFrameCounter--;
        }
        initialized = initializeFrameCounter == 0;
    }
}
