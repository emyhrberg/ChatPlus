using System;
using System.Collections.Generic;
using ChatPlus.Core.Chat;
using ChatPlus.Core.Features.Items;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Terraria;

namespace ChatPlus.Core.Features.Glyphs;

public class GlyphPanel : BasePanel<Glyph>
{
    protected override BaseElement<Glyph> BuildElement(Glyph data) =>
        new GlyphElement(data);

    protected override IEnumerable<Glyph> GetSource() =>
        GlyphManager.Glyphs;

    protected override string GetDescription(Glyph data)
        => data.Description;

    protected override string GetTag(Glyph data)
        => data.Tag;


    public override void Update(GameTime gt)
    {
        base.Update(gt);
    }

    protected override bool MatchesFilter(Glyph data)
    {
        string text = Main.chatText ?? string.Empty;
        int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);

        string query = GetQuery(text, caret, "[g");
        if (string.IsNullOrEmpty(query))
        {
            return true;
        }

        string tag = data.Tag ?? string.Empty;
        string desc = data.Description ?? string.Empty;

        return tag.Contains(query, StringComparison.OrdinalIgnoreCase)
            || desc.Contains(query, StringComparison.OrdinalIgnoreCase);

        static string GetQuery(string source, int caretPos, string prefix)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            int startSearch = Math.Max(0, Math.Min(caretPos - 1, source.Length - 1));
            int s = source.LastIndexOf(prefix, startSearch, startSearch + 1, StringComparison.OrdinalIgnoreCase);
            if (s < 0)
            {
                return string.Empty;
            }

            int start = s + prefix.Length;

            if (start < source.Length)
            {
                char ch = source[start];
                if (ch == ':' || ch == '/')
                {
                    start++;
                }
            }

            int close = source.IndexOf(']', start);

            if (close >= 0 && caretPos > close)
            {
                return string.Empty; // token already finished; caret is beyond -> no filtering
            }

            int end = close >= 0 ? Math.Min(caretPos, close) : caretPos;
            if (end <= start)
            {
                return string.Empty;
            }

            return source.Substring(start, end - start).Trim();
        }
    }
}
