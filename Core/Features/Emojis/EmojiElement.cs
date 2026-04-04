using System.Linq;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI.Chat;
using static ChatPlus.Common.Configs.ClientConfig;

namespace ChatPlus.Core.Features.Emojis;

public class EmojiElement : BaseElement<Emoji>
{
    private readonly Emoji emoji;
    private readonly EmojiIconImage image;

    public EmojiElement(Emoji emoji) : base(emoji)
    {
        this.emoji = emoji;
        image = new EmojiIconImage(emoji.FilePath);
        Append(image);
        Height.Set(30, 0);
        Width.Set(0, 1f);
    }
    protected override void DrawListElement(SpriteBatch sb)
    {
        var dims = GetDimensions();
        Vector2 pos = dims.Position() + new Vector2(32, 4);

        if (EmojiSystem.OpenedFromColon)
        {
            string ExtractSimpleName(string tag, string description)
            {
                if (!string.IsNullOrWhiteSpace(description))
                    return description.Trim();

                if (string.IsNullOrEmpty(tag))
                    return string.Empty;

                if (tag.StartsWith("[e:") && tag.EndsWith("]"))
                {
                    int colon = tag.IndexOf(':');
                    int close = tag.LastIndexOf(']');
                    if (colon >= 0 && close > colon + 1)
                        return tag.Substring(colon + 1, close - colon - 1);
                }

                if (tag.StartsWith(":"))
                    return tag.TrimStart(':');

                return tag;
            }

            string simple = ExtractSimpleName(emoji.Tag, emoji.Description);
            var snip = new TextSnippet[] { new TextSnippet(":" + simple) };
            ChatManager.DrawColorCodedStringWithShadow(sb,FontAssets.MouseText.Value,snip,pos,0f, Vector2.Zero,Vector2.One,out _);
        }
        else
        {
            var tagSnip = new TextSnippet[] { new TextSnippet(emoji.Synonyms.FirstOrDefault()) };

            ChatManager.DrawColorCodedStringWithShadow(sb,FontAssets.MouseText.Value, tagSnip,pos,0f, Vector2.Zero,Vector2.One,out _ );
        }
    }
    protected override void DrawGridElement(SpriteBatch sb)
    {
        // do nothing!
    }
}
