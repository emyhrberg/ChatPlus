using ChatPlus.Common.Debug;
using System;
using System.Text.RegularExpressions;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Links;

public class LinkTagHandler : ITagHandler
{
    public static string GenerateTag(string url)
    {
        return $"[link:{url}]";
    }

    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
    {
        Log.Debug("Parsing link from text: " + text);

        return new LinkSnippet(text, text, baseColor);
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
