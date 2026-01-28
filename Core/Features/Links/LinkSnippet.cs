using System;
using System.Diagnostics;
using ChatPlus.Common.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Links;

public sealed class LinkSnippet : TextSnippet
{
    private readonly string url;
    private uint lastHoverFrame;

    public LinkSnippet(string displayText, string url, Color baseColor)
        : base(displayText, baseColor)
    {
        this.url = url;
        CheckForHover = true;
    }

    public override void OnHover()
    {
        lastHoverFrame = Main.GameUpdateCount;
        Main.LocalPlayer.mouseInterface = true;

        // Optional tooltip:
        // UICommon.TooltipMouseText(url);
    }

    public override void OnClick()
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Main.NewText("Failed to open link: " + ex.Message, Color.Red);
            Log.Error("Failed to open link: " + ex);
        }
    }

    public override Color GetVisibleColor()
    {
        if (lastHoverFrame == Main.GameUpdateCount)
        {
            return new Color(6, 69, 173);
        }

        return new Color(0, 125, 255);
    }

    // Preserve URL when the chat system splits the snippet across lines.
    public override TextSnippet CopyMorph(string newText)
    {
        return new LinkSnippet(newText, url, Color);
    }

    // Optional underline when hovered.
    public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch sb, Vector2 pos = default, Color passColor = default, float scale = 1f)
    {
        var font = FontAssets.MouseText.Value;
        size = ChatManager.GetStringSize(font, Text, new Vector2(scale));

        if (justCheckingString)
        {
            return false;
        }

        bool isShadowPass = passColor.R + passColor.G + passColor.B <= 5;
        if (isShadowPass)
        {
            return false;
        }

        if (lastHoverFrame != Main.GameUpdateCount)
        {
            return false;
        }

        int width = (int)Math.Ceiling(size.X);
        int lineHeight = (int)Math.Ceiling(font.LineSpacing * scale);
        int underlineY = (int)Math.Floor(pos.Y + lineHeight - 2f);

        var underlineRect = new Rectangle((int)pos.X, underlineY, width, 2);
        sb.Draw(TextureAssets.MagicPixel.Value, underlineRect, GetVisibleColor());

        return false; // let vanilla draw the text
    }
}
