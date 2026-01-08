using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Uploads;

public sealed class UploadTagHandler : ITagHandler
{
    private static readonly Dictionary<string, Texture2D> Registry =
        new(StringComparer.OrdinalIgnoreCase);

    public static bool TryGet(string key, out Texture2D tex) => Registry.TryGetValue(key, out tex);
    public static void Clear() => Registry.Clear();
    public static string GenerateTag(string key) => $"[u:{key}]";
    public static bool Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;
        return Registry.Remove(key.Trim());
    }
    public static bool Register(string key, Texture2D texture)
    {
        if (texture == null || string.IsNullOrWhiteSpace(key))
            return false;

        Registry[key] = texture;
        return true;
    }

    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
    {
        // allow keys with spaces: parser gives part in 'text' and the rest in 'options' (if it doesn’t contain '=').
        string key = text ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(options) && options.IndexOf('=') < 0)
            key = $"{key} {options}".Trim();

        key = key.Trim().TrimEnd(']'); // tolerate stray closers

        if (Registry.TryGetValue(key, out var texture))
        {
            return new UploadSnippet(key, texture)
            {
                Text = GenerateTag(key)
            };
        }

        // Handle multiplayer
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            UploadNetHandler.Request(key);
        }

        // If no tag was found, return the regular snippet
        return new TextSnippet(text ?? string.Empty);
    }

    // helper
    public static bool ContainsUploadTag(string text)
    {
        return Regex.IsMatch(text, @"\[u:[^\]]+\]", RegexOptions.IgnoreCase);
    }
}
