using ChatPlus.Common.Configs;
using ChatPlus.Core.Features.Stats.UploadStats;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Uploads;

public class UploadSnippet : TextSnippet
{
    private readonly Texture2D tex;
    private readonly string key;

    public UploadSnippet(string key, Texture2D texture)
    {
        this.key = key;
        tex = texture;
    }

    public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch sb, Vector2 pos = default, Color color = default, float scale = 1f)
    {
        float targetH = 147f * scale;
        if (tex == null || tex.Height <= 0) { size = new Vector2(targetH, targetH); return true; }

        float s = targetH / tex.Height;
        float w = tex.Width * s;

        size = new Vector2(w, targetH);
        if (justCheckingString) return true;

        sb.Draw(tex, pos, null, Color, 0f, Vector2.Zero, s, SpriteEffects.None, 0f);

        Rectangle bounds = new((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
        bool hovering = bounds.Contains(Main.MouseScreen.ToPoint());

        if (hovering)
        {
            UICommon.TooltipMouseText(Text);
            if (Main.mouseLeft && Main.mouseLeftRelease)
            {
                Main.mouseLeftRelease = false;
                var upload = new Upload(Text, key, $"InMemory:{key}", tex);
                UploadInfoState.Instance?.Show(upload);
            }
        }

        return true;
    }
}
