using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.Emojis;
public class EmojiIconImage : UIImage
{
    private Asset<Texture2D> emojiAsset;
    private string emojiFilePath;

    private bool requested;

    public EmojiIconImage(string filePath) : base(TextureAssets.MagicPixel.Value)
    {
        emojiFilePath = filePath;
        Width.Set(24, 0);
        Height.Set(24, 0);
        Top.Set(2, 0);
        Left.Set(4, 0);
    }
    public override void Draw(SpriteBatch sb)
    {
        Top.Set(2, 0);

        if (!requested)
        {
            requested = true;
            if (ModContent.HasAsset(emojiFilePath))
                emojiAsset = ModContent.Request<Texture2D>(emojiFilePath);
        }

        var tex = emojiAsset?.Value;
        if (tex != null)
        {
            // Draw the emoji icon
            Rectangle target = GetDimensions().ToRectangle();
            DrawTextureScaledToFit(sb, tex, target);
        }
        else
        {
            base.Draw(sb);
        }
    }

    private static void DrawTextureScaledToFit(SpriteBatch sb, Texture2D tex, Rectangle target)
    {
        if (tex == null)
            return;

        float scale = Math.Min(
            target.Width / (float)tex.Width,
            target.Height / (float)tex.Height
        );

        sb.Draw(
            tex,
            target.Center.ToVector2(),
            null,
            Color.White,
            0f,
            tex.Size() / 2f,
            scale,
            SpriteEffects.None,
            0f
        );
    }
}