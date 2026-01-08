using System;
using System.Collections.Generic;
using ChatPlus.Core.Chat;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Terraria;

namespace ChatPlus.Core.Features.Emojis;
public class EmojiPanel : BasePanel<Emoji>
{
    protected override BaseElement<Emoji> BuildElement(Emoji data) => new EmojiElement(data);

    protected override IEnumerable<Emoji> GetSource() => EmojiManager.Emojis;

    protected override string GetDescription(Emoji data) => data.Description;

    protected override string GetTag(Emoji data) => data.Tag;

    protected override bool MatchesFilter(Emoji data)
    {
        string text = Main.chatText ?? string.Empty;
        int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);

        string query = GetEmojiQuery(text, caret);

        if (string.IsNullOrEmpty(query))
        {
            return true;
        }

        string key = ExtractKeyFromTag(data.Tag);
        if (string.IsNullOrEmpty(key))
        {
            key = data.Description;
        }

        if (!string.IsNullOrEmpty(key))
        {
            if (key.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        if (data.Synonyms != null)
        {
            foreach (string syn in data.Synonyms)
            {
                if (string.IsNullOrEmpty(syn))
                {
                    continue;
                }

                if (syn.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
        }

        return false;


        static string GetEmojiQuery(string source, int caretPos)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            int colon = -1;
            if (caretPos > 0)
            {
                colon = source.LastIndexOf(':', caretPos - 1);
            }

            bool colonMode = false;
            if (colon >= 0)
            {
                int lb = source.LastIndexOf('[', colon);
                int rb = source.LastIndexOf(']', colon);
                colonMode = lb <= rb;
            }

            if (colonMode)
            {
                int qStart = colon + 1;
                int qLen = caretPos - qStart;
                if (qLen <= 0)
                {
                    return string.Empty;
                }

                return source.Substring(qStart, qLen);
            }

            int eStart = source.LastIndexOf("[e", StringComparison.OrdinalIgnoreCase);
            if (eStart >= 0 && caretPos >= eStart)
            {
                int closing = source.IndexOf(']', eStart + 2);
                bool eOpen = closing == -1;
                if (eOpen)
                {
                    int qStart = eStart + 2;

                    if (qStart < source.Length)
                    {
                        char ch = source[qStart];
                        if (ch == ':' || ch == '/')
                        {
                            qStart++;
                        }
                    }

                    int qLen = caretPos - qStart;
                    if (qLen <= 0)
                    {
                        return string.Empty;
                    }

                    return source.Substring(qStart, qLen).Trim();
                }
            }

            return string.Empty;
        }

        static string ExtractKeyFromTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return null;
            }

            int c = tag.IndexOf(':');
            int r = tag.LastIndexOf(']');
            if (c >= 0 && r > c)
            {
                return tag.Substring(c + 1, r - c - 1);
            }

            if (tag.StartsWith("[") && tag.EndsWith("]"))
            {
                return tag.Substring(1, tag.Length - 2);
            }

            return tag;
        }
    }

    public override void InsertSelectedTag()
    {
        EmojiState.WasOpenedByButton = false;

        if (items.Count == 0)
        {
            return;
        }

        if (currentIndex < 0)
        {
            return;
        }

        Emoji emoji = items[currentIndex].Data;

        string insert = ComputeInsert(emoji);
        if (string.IsNullOrEmpty(insert))
        {
            return;
        }

        string text = Main.chatText ?? string.Empty;
        int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);

        int colon = -1;
        if (caret > 0)
        {
            colon = text.LastIndexOf(':', caret - 1);
        }

        bool colonMode = false;
        if (colon >= 0)
        {
            int lb = text.LastIndexOf('[', colon);
            int rb = text.LastIndexOf(']', colon);
            colonMode = lb <= rb;
        }

        if (colonMode)
        {
            string before = text.Substring(0, colon);
            string after = text.Substring(caret);
            Main.chatText = before + insert + after;
            HandleChatSystem.SetCaretPos(before.Length + insert.Length);
            return;
        }

        int eStart = text.LastIndexOf("[e", StringComparison.OrdinalIgnoreCase);
        if (eStart >= 0 && caret >= eStart)
        {
            int closing = text.IndexOf(']', eStart + 2);
            bool eOpen = closing == -1;
            if (eOpen)
            {
                string before = text.Substring(0, eStart);
                string after = text.Substring(caret);
                Main.chatText = before + insert + after;
                HandleChatSystem.SetCaretPos(before.Length + insert.Length);
                return;
            }
        }

        Main.chatText += insert;
        HandleChatSystem.SetCaretPos(Main.chatText.Length);

        static string ComputeInsert(in Emoji e)
        {
            if (!string.IsNullOrEmpty(e.Tag))
            {
                string t = e.Tag.Trim();
                if (t.StartsWith("[e", StringComparison.OrdinalIgnoreCase) && t.EndsWith("]"))
                {
                    return t;
                }

                string keyFromTag = ExtractKeyFromTag(t);
                if (!string.IsNullOrEmpty(keyFromTag))
                {
                    return "[e:" + keyFromTag + "]";
                }
            }

            if (!string.IsNullOrEmpty(e.Description))
            {
                string key = NormalizeKey(e.Description);
                if (!string.IsNullOrEmpty(key))
                {
                    return "[e:" + key + "]";
                }
            }

            return null;
        }

        static string ExtractKeyFromTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return null;
            }

            int c = tag.IndexOf(':');
            int r = tag.LastIndexOf(']');
            if (c >= 0 && r > c)
            {
                return tag.Substring(c + 1, r - c - 1);
            }

            if (tag.StartsWith("[e", StringComparison.OrdinalIgnoreCase))
            {
                int colonIndex = tag.IndexOf(':');
                if (colonIndex >= 0 && colonIndex + 1 < tag.Length)
                {
                    string tail = tag.Substring(colonIndex + 1);
                    if (tail.EndsWith("]"))
                    {
                        tail = tail.Substring(0, tail.Length - 1);
                    }
                    return tail.Trim();
                }
            }

            if (tag.StartsWith("[") && tag.EndsWith("]"))
            {
                return tag.Substring(1, tag.Length - 2).Trim();
            }

            return tag.Trim();
        }

        static string NormalizeKey(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            s = s.Trim().ToLowerInvariant();

            var chars = new List<char>(s.Length);
            bool lastUnderscore = false;

            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];

                if (char.IsWhiteSpace(ch) || ch == '-')
                {
                    if (!lastUnderscore)
                    {
                        chars.Add('_');
                        lastUnderscore = true;
                    }
                    continue;
                }

                lastUnderscore = false;
                chars.Add(ch);
            }

            return new string(chars.ToArray());
        }
    }

    public override void Update(GameTime gt)
    {
        base.Update(gt);
    }
}
