using System;
using System.Diagnostics;
using ChatPlus.Common.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Links;

public sealed class LinkSnippet : TextSnippet
{
    private readonly string url;
    private uint lastHoverFrame;
    private bool hovered;

    public LinkSnippet(string displayText, string url, Color baseColor)
        : base(displayText, baseColor)
    {
        this.url = url;
        CheckForHover = true;
    }

    public override void OnHover()
    {
        Main.LocalPlayer.mouseInterface = true;
        //UICommon.TooltipMouseText(url);
        Main.instance.MouseText("Open link", hackedMouseY: 20);
        LinkCursorOverrideSystem.WantLinkCursor = true;
    }

    public override void OnClick()
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Main.NewText("Failed to open link: " + ex.Message, Color.Red);
            Log.Error("Failed to open link: " + ex);
        }
    }

    public override Color GetVisibleColor()
    {
        return hovered ? new Color(6, 69, 173) : new Color(0, 125, 255);
    }

    public override TextSnippet CopyMorph(string newText)
    {
        return new LinkSnippet(newText, url, Color);
    }

    public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch sb, Vector2 pos = default, Color passColor = default, float scale = 1f)
    {
        var font = FontAssets.MouseText.Value;
        size = ChatManager.GetStringSize(font, Text, new Vector2(scale));

        if (justCheckingString)
            return false;

        bool isShadowPass = passColor.R + passColor.G + passColor.B <= 5;
        if (isShadowPass)
            return false;

        var rect = new Rectangle(
            (int)Math.Floor(pos.X),
            (int)Math.Floor(pos.Y),
            (int)Math.Ceiling(size.X),
            (int)Math.Ceiling(font.LineSpacing * scale)-11);

        // debug: draw hover rectangle
        //sb.Draw(TextureAssets.MagicPixel.Value, rect, Color.Red * 0.35f);

        hovered = rect.Contains(Main.MouseScreen.ToPoint());

        if (hovered)
        {
            Main.LocalPlayer.mouseInterface = true;

            if (Main.mouseLeft && Main.mouseLeftRelease)
            {
                Main.mouseLeftRelease = false;
                OnClick();
            }

            int underlineY = rect.Bottom;
            var underlineRect = new Rectangle(rect.X, underlineY, rect.Width, 2);
            sb.Draw(TextureAssets.MagicPixel.Value, underlineRect, GetVisibleColor());
        }

        // debug: comment this out
        LinkCursorOverrideSystem.WantLinkCursor = false;

        return false; // let vanilla draw text
    }
}

