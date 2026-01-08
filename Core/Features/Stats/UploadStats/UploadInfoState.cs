using System;
using ChatPlus.Core.Features.Stats.Base;
using ChatPlus.Core.Features.Uploads;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI; 

namespace ChatPlus.Core.Features.Stats.UploadStats;
public class UploadInfoState : BaseInfoState, ILoadable
{
    public static UploadInfoState Instance;

    private Upload? _current;
    public void Load(Mod mod) => Instance = this;
    public void Unload() => Instance = null;

    protected override void DrawContent(SpriteBatch sb, Rectangle inner)
    {
        if (_current == null || _current.Value.Texture == null)
            return;

        var upload = _current.Value;
        if (TitlePanel.Text != upload.FileName)
            SetTitle(upload.FileName);

        var tex = _current.Value.Texture;
        int srcW = tex.Width;
        int srcH = tex.Height;

        // Normal fit scale
        float fitScale = MathF.Min(
            inner.Width / (float)srcW,
            inner.Height / (float)srcH
        );

        // If image is "small" (both dimensions under 100), don’t upscale
        float scale;
        if (srcW < 100 && srcH < 100)
        {
            scale = Math.Min(10f, fitScale); // don’t exceed 1x
        }
        else
        {
            scale = fitScale;
        }

        int drawW = (int)(srcW * scale);
        int drawH = (int)(srcH * scale);

        var pos = new Vector2(
            inner.X + (inner.Width - drawW) / 2f,
            inner.Y + (inner.Height - drawH) / 2f
        );

        sb.Draw(tex, new Rectangle((int)pos.X, (int)pos.Y, drawW, drawH), Color.White);
    }

    public void Show(Upload upload)
    {
        _current = upload;
        SetTitle(upload.FileName);
        OpenForCurrentContext();
    }

    public void Close()
    {
        _current = null;
        CloseForCurrentContext();
    }
}
