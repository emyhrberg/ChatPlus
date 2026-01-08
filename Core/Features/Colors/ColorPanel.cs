using System;
using System.Collections.Generic;
using ChatPlus.Core.Chat;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Terraria;

namespace ChatPlus.Core.Features.Colors;
public class ColorPanel : BasePanel<ColorEntry>
{
    protected override BaseElement<ColorEntry> BuildElement(ColorEntry data) => new ColorElement(data);
    protected override IEnumerable<ColorEntry> GetSource() => ColorManager.Colors;
    protected override string GetDescription(ColorEntry data) => data.Description;
    protected override string GetTag(ColorEntry data) => data.Tag;

    public override void Update(GameTime gt)
    {
        base.Update(gt);
    }

    public override void InsertSelectedTag()
    {
        if (items.Count == 0)
        {
            return;
        }

        if (currentIndex < 0)
        {
            return;
        }

        string tag = GetTag(items[currentIndex].Data);
        if (string.IsNullOrEmpty(tag))
        {
            return;
        }

        string text = Main.chatText ?? string.Empty;

        var sel = HandleChatSystem.GetSelection();

        int start = 0;
        int end = 0;

        if (sel != null)
        {
            start = Math.Clamp(sel.Value.start, 0, text.Length);
            end = Math.Clamp(sel.Value.end, 0, text.Length);
        }
        else
        {
            int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);
            start = caret;
            end = caret;
        }

        string prefix = tag;   // e.g. "[c/ffffff:"
        string suffix = "]";

        int anchor = start;

        static bool IsHex(char ch)
        {
            if (ch >= '0' && ch <= '9')
            {
                return true;
            }

            if (ch >= 'a' && ch <= 'f')
            {
                return true;
            }

            if (ch >= 'A' && ch <= 'F')
            {
                return true;
            }

            return false;
        }

        static bool LooksLikeUnfinishedColorTag(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            if (s[0] != '[')
            {
                return false;
            }

            if (s.Length < 2)
            {
                return false;
            }

            char c = s[1];
            if (c != 'c' && c != 'C')
            {
                return false;
            }

            for (int i = 2; i < s.Length; i++)
            {
                char ch = s[i];
                if (ch == '/')
                {
                    continue;
                }

                if (ch == ':')
                {
                    if (i != s.Length - 1)
                    {
                        return false;
                    }

                    continue;
                }

                if (IsHex(ch))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        int openIdx = -1;
        for (int i = anchor - 1; i >= 0; i--)
        {
            char ch = text[i];

            if (ch == ']')
            {
                break;
            }

            if (ch == '[')
            {
                openIdx = i;
                break;
            }
        }

        bool hasUnfinished = false;
        if (openIdx >= 0)
        {
            string candidate = text.Substring(openIdx, anchor - openIdx);
            if (LooksLikeUnfinishedColorTag(candidate))
            {
                hasUnfinished = true;
            }
        }

        if (end > start)
        {
            if (hasUnfinished)
            {
                string before = text.Substring(0, openIdx);
                string mid = text.Substring(start, end - start);
                string after = end < text.Length ? text.Substring(end) : string.Empty;

                string wrapped = prefix + mid + suffix;

                Main.chatText = before + wrapped + after;
                HandleChatSystem.SetCaretPos(before.Length + wrapped.Length);
            }
            else
            {
                string before = text.Substring(0, start);
                string mid = text.Substring(start, end - start);
                string after = end < text.Length ? text.Substring(end) : string.Empty;

                string wrapped = prefix + mid + suffix;

                Main.chatText = before + wrapped + after;
                HandleChatSystem.SetCaretPos(before.Length + wrapped.Length);
            }

            return;
        }

        if (hasUnfinished)
        {
            string before = text.Substring(0, openIdx);
            string after = anchor < text.Length ? text.Substring(anchor) : string.Empty;

            string insertion = prefix;

            Main.chatText = before + insertion + after;
            HandleChatSystem.SetCaretPos(before.Length + prefix.Length);
            return;
        }
        else
        {
            int caret = start;
            string before = text.Substring(0, caret);
            string after = caret < text.Length ? text.Substring(caret) : string.Empty;

            string insertion = prefix + suffix;

            Main.chatText = before + insertion + after;
            HandleChatSystem.SetCaretPos(before.Length + prefix.Length);
            return;
        }
    }

    protected override bool MatchesFilter(ColorEntry data)
    {
        string text = Main.chatText ?? string.Empty;
        int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);

        string query = GetColorQuery(text, caret);
        if (string.IsNullOrEmpty(query))
        {
            return true;
        }

        string hexKey = ExtractHexFromTag(data.Tag);
        if (!string.IsNullOrEmpty(hexKey))
        {
            if (hexKey.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        if (!string.IsNullOrEmpty(data.Description))
        {
            if (data.Description.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;

        static string GetColorQuery(string source, int caretPos)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            int lb = -1;
            if (caretPos > 0)
            {
                lb = source.LastIndexOf('[', caretPos - 1);
            }

            if (lb < 0)
            {
                return string.Empty;
            }

            int rb = -1;
            if (caretPos > 0)
            {
                rb = source.LastIndexOf(']', caretPos - 1);
            }

            bool insideOpenBracket = lb > rb;
            if (!insideOpenBracket)
            {
                return string.Empty;
            }

            if (lb + 2 > source.Length - 1)
            {
                return string.Empty;
            }

            char c1 = source[lb + 1];
            if (c1 != 'c' && c1 != 'C')
            {
                return string.Empty;
            }

            int qStart = lb + 2;

            if (qStart < source.Length && source[qStart] == '/')
            {
                qStart++;
            }

            int qEnd = caretPos;

            for (int i = qStart; i < caretPos; i++)
            {
                char ch = source[i];
                if (ch == ':')
                {
                    qEnd = i;
                    break;
                }

                if (ch == ']')
                {
                    return string.Empty;
                }

                if (!IsHex(ch))
                {
                    return string.Empty;
                }
            }

            int len = qEnd - qStart;
            if (len <= 0)
            {
                return string.Empty;
            }

            return source.Substring(qStart, len);

            static bool IsHex(char ch)
            {
                if (ch >= '0' && ch <= '9')
                {
                    return true;
                }

                if (ch >= 'a' && ch <= 'f')
                {
                    return true;
                }

                if (ch >= 'A' && ch <= 'F')
                {
                    return true;
                }

                return false;
            }
        }

        static string ExtractHexFromTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return null;
            }

            int slash = tag.IndexOf('/');
            if (slash < 0 || slash + 1 >= tag.Length)
            {
                return null;
            }

            int stop = tag.IndexOf(':', slash + 1);
            if (stop < 0)
            {
                stop = tag.IndexOf(']', slash + 1);
            }

            if (stop < 0)
            {
                stop = tag.Length;
            }

            string hex = tag.Substring(slash + 1, stop - slash - 1);
            return hex;
        }
    }

}
