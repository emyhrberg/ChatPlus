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
        string key = text ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(options) && options.IndexOf('=') < 0)
            key = $"{key} {options}".Trim();

        key = key.Trim().TrimEnd(']');

        // Always return an UploadSnippet so it can start drawing as soon as the texture arrives.
        if (!Registry.ContainsKey(key) && Main.netMode != NetmodeID.SinglePlayer)
            UploadNetHandler.RequestOnce(key);

        return new UploadSnippet(key)
        {
            Text = GenerateTag(key),
            Color = baseColor
        };
    }

    // helper
    public static bool ContainsUploadTag(string text)
    {
        return Regex.IsMatch(text, @"\[u:[^\]]+\]", RegexOptions.IgnoreCase);
    }
}
