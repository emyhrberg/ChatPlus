using ChatPlus.Core.Features.Stats.UploadStats;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Uploads;

public class UploadSnippet : TextSnippet
{
    private readonly string _key;

    public UploadSnippet(string key)
    {
        _key = key;
    }

    public override bool UniqueDraw(
        bool justCheckingString,
        out Vector2 size,
        SpriteBatch sb,
        Vector2 pos = default,
        Color color = default,
        float scale = 1f)
    {
        float box = 147f * scale;
        size = new Vector2(box, box);

        if (justCheckingString)
            return true;

        if (!UploadTagHandler.TryGet(_key, out var tex) || tex == null || tex.IsDisposed)
        {
            if (Main.netMode != NetmodeID.SinglePlayer)
                UploadNetHandler.RequestOnce(_key);

            DrawPlaceholder(sb, pos, box);
            return true;
        }

        float s = Math.Min(box / tex.Width, box / tex.Height);
        if (s > 1f)
            s = 1f;

        float drawW = tex.Width * s;
        float drawH = tex.Height * s;

        Vector2 drawPos = pos + new Vector2((box - drawW) * 0.5f, (box - drawH) * 0.5f);
        sb.Draw(tex, drawPos, null, color, 0f, Vector2.Zero, s, SpriteEffects.None, 0f);

        Rectangle bounds = new((int)pos.X, (int)pos.Y, (int)box, (int)box);
        bool hovering = bounds.Contains(Main.MouseScreen.ToPoint());

        if (hovering)
        {
            UICommon.TooltipMouseText(Text);

            if (Main.mouseLeft && Main.mouseLeftRelease)
            {
                Main.mouseLeftRelease = false;

                var upload = new Upload(Text, _key, $"InMemory:{_key}", tex);
                UploadInfoState.Instance?.Show(upload);
            }
        }

        return true;
    }

    private static void DrawPlaceholder(SpriteBatch sb, Vector2 pos, float box)
    {
        var rect = new Rectangle((int)pos.X, (int)pos.Y, (int)box, (int)box);
        sb.Draw(TextureAssets.MagicPixel.Value, rect, new Color(255, 255, 255, 30));

        ChatManager.DrawColorCodedStringWithShadow(
            sb,
            FontAssets.MouseText.Value,
            "Loading…",
            pos + new Vector2(6f, 6f),
            Color.Gray,
            0f,
            Vector2.Zero,
            Vector2.One * 0.75f
        );
    }
}
