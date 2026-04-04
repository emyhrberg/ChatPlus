using System;
using System.IO;
using ChatPlus.Core.Features.Glyphs;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;
using static ChatPlus.Common.Configs.ClientConfig;

namespace ChatPlus.Core.Features.Uploads;

public class UploadElement : BaseElement<Upload>
{
    public Upload Element;

    private UIText img;

    public UploadElement(Upload data) : base(data)
    {
        Element = data;
        Height.Set(60, 0);
        Width.Set(0, 1);

        img = new(Element.Tag);
        img._textScale = 0.3f;
        img.Left.Set(5, 0);
        img.Top.Set(8, 0);
        Append(img);
    }
    public override void LeftClick(UIMouseEvent evt)
    {
        bool shift = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
        if (shift)
        {
            var panel = GetParentPanel() as UploadPanel;
            if (UploadManager.TryDelete(Data))
            {
                panel?.PopulatePanel();
                Main.NewText($"Deleted {Data.FileName}", Color.OrangeRed);
            }
            return;
        }

        base.LeftClick(evt);
    }

    protected override void DrawGridElement(SpriteBatch sb)
    {
        var tex = Data.Texture;
        if (tex == null || tex.IsDisposed)
            return;

        var dims = GetDimensions();
        Rectangle cell = dims.ToRectangle();

        int pad = 4;
        int innerW = Math.Max(0, cell.Width - pad * 2);
        int innerH = Math.Max(0, cell.Height - pad * 2);

        int srcW = tex.Width;
        int srcH = tex.Height;

        if (srcW <= 0 || srcH <= 0 || innerW <= 0 || innerH <= 0)
        {
            return;
        }

        // Scale to fit inside the inner rectangle (letterbox)
        float scaleX = (float)innerW / (float)srcW;
        float scaleY = (float)innerH / (float)srcH;
        float scale = Math.Min(scaleX, scaleY);

        if (scale > 1f)
            scale = 1f;

        int drawW = (int)Math.Round(srcW * scale);
        int drawH = (int)Math.Round(srcH * scale);

        // Center within the cell
        int drawX = cell.X + (cell.Width - drawW) / 2;
        int drawY = cell.Y + (cell.Height - drawH) / 2;

        var dest = new Rectangle(drawX, drawY, drawW, drawH);
        sb.Draw(tex, dest, Color.White);
    }

    protected override void DrawListElement(SpriteBatch sb)
    {
        var tex = Data.Texture;
        if (tex == null || tex.IsDisposed)
            return;

        var dims = GetDimensions();
        Vector2 pos = dims.Position();

        if (tex != null && tex.Height > 0)
        {
            float scale = 30f / tex.Height;
            scale *= 0.8f;
            if (scale > 1f)
                scale = 1f;

            int drawW = (int)(tex.Width * scale);
            int drawH = (int)(tex.Height * scale);

            var dest = new Rectangle((int)pos.X + 3, (int)pos.Y + 3, drawW, drawH);
            sb.Draw(tex, dest, Color.White);
        }

        string fileName = Path.GetFileNameWithoutExtension(Data.FileName);
        ChatManager.DrawColorCodedStringWithShadow(
            sb, FontAssets.MouseText.Value, fileName,
            pos + new Vector2(65, 5), Color.White, 0f, Vector2.Zero, Vector2.One
        );
    }

}
