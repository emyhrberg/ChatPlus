using System;
using System.Text.RegularExpressions;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Links;

internal sealed class LinkTagHandler : ITagHandler
{
    public static string GenerateTag(string url)
    {
        return $"[l:{url}]";
    }

    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
    {
        // If options is present and doesn't look like key=value, Terraria sometimes splits it out.
        string raw = text ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(options) && options.IndexOf('=') < 0)
        {
            raw = $"{raw} {options}".Trim();
        }

        raw = raw.Trim().TrimEnd(']');

        // Normalize *once* and store; do not rely on Text later.
        string normalized = NormalizeUrl(raw);

        return new LinkSnippet(displayText: raw, url: normalized, baseColor);
    }

    internal static string NormalizeUrl(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        string url = raw.Trim();

        // Trim common trailing punctuation that your regex often captures.
        url = url.TrimEnd('.', ',', ';', ':', '!', '?', ')', ']', '}', '"', '\'');

        if (url.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }

        return url;
    }

    public static bool ContainsLink(string text) 
    { 
        return Regex.IsMatch(text, @"(https?://|www\.)\S+\.\S+", RegexOptions.IgnoreCase); 
    }
    public static bool TryGetLink(string input, out string link) 
    { 
        var match = Regex.Match(input, @"(https?://|www\.)\S+\.\S+", RegexOptions.IgnoreCase); 
        if (match.Success) 
        {
            link = match.Value;
            return true; 
        } 
        
        link = null; 
        return false;
    }
}
