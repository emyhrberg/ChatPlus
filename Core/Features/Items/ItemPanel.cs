using System;
using System.Collections.Generic;
using System.Linq;
using ChatPlus.Core.Chat;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Terraria;

namespace ChatPlus.Core.Features.Items;

public class ItemPanel : BasePanel<ItemEntry>
{
    protected override BaseElement<ItemEntry> BuildElement(ItemEntry data) => new ItemElement(data);

    protected override IEnumerable<ItemEntry> GetSource() => ItemManager.Items.OrderBy(i => i.ID);

    protected override string GetDescription(ItemEntry data)
    {
        return $"{data.DisplayName}";
    }

    protected override string GetTag(ItemEntry data) => data.Tag;

    public override void Update(GameTime gt)
    {
        base.Update(gt);
    }

    protected override bool MatchesFilter(ItemEntry data)
    {
        string text = Main.chatText ?? string.Empty;
        int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);

        string query = GetQuery(text, caret, "[i");
        if (string.IsNullOrEmpty(query))
        {
            return true;
        }

        string tag = data.Tag ?? string.Empty;
        if (tag.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string display = data.DisplayName ?? string.Empty;
        if (display.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (int.TryParse(query, out int qid) && qid == data.ID)
        {
            return true;
        }

        return false;

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
