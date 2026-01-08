using System;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Mentions;

internal class MentionTagHandler : ITagHandler
{
    public static string GenerateTag(string playerName) => $"[mention:{playerName}]";
    public static string GenerateTag(string playerName, int width) => $"[mention:{playerName}/width:{width}]";
    public TextSnippet Parse(string text, Color baseColor = default, string options = null)
    {
        // text is "Name" or "Name/width:300"
        int w = -1;
        string name = text;

        int slash = text.IndexOf("/width:", StringComparison.OrdinalIgnoreCase);
        if (slash >= 0)
        {
            name = text.Substring(0, slash);
            var span = text.AsSpan(slash + 7);
            int i = 0;
            while (i < span.Length && char.IsDigit(span[i])) i++;
            if (int.TryParse(span[..i], out int parsed)) w = parsed;
        }
        var c = baseColor == default ? Color.White : baseColor;
        return new MentionSnippet(new TextSnippet(name, c), w);
    }
}

