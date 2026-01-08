using System;
using System.Collections.Generic;
using System.Security.Policy;
using ChatPlus.Common;
using ChatPlus.Common.Debug;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.ModIcons;

/// <summary>
/// Tag handler for mod icons: [m:InternalName]
/// </summary>
public sealed class ModIconTagHandler : ITagHandler
{
    /// <summary>
    /// The registry stores tags (e.g [m:ModReloader] and their associated textures.
    /// </summary>
    private static readonly Dictionary<string, Texture2D> Registry = new(StringComparer.OrdinalIgnoreCase);
    public static void Clear() => Registry.Clear();
    public static string GenerateTag(string internalName) => $"[m:{internalName}]";
    private static int modCount = 0;
    public static bool Register(Mod mod)
    {
        string internalName = mod.Name;

        Texture2D tex = null;
        string smallPath = $"{internalName}/icon_small";
        string normalPath = $"{internalName}/icon";
        bool smallExists = false;

        if (ModContent.HasAsset(smallPath))
        {
            smallExists = true;
            tex = ModContent.Request<Texture2D>(smallPath).Value;
        }
        else if (ModContent.HasAsset(normalPath))
        {
            tex = ModContent.Request<Texture2D>(normalPath).Value;
        }
        else if (internalName.Equals("ModLoader", StringComparison.OrdinalIgnoreCase))
        {
            tex = Ass.tModLoaderIcon.Value;
        }
        else
        {
            tex = null; // no icon found, means we draw fallback initials later
        }

        Registry[internalName] = tex;
        modCount++;
        Log.Info($"({modCount}) Mod icon: '{internalName}' loaded. Texture: {tex != null}, Small Icon Exists: {smallExists}");
        return true;
    }

    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
    {
        string key = text?.Trim() ?? string.Empty;
        return new ModIconSnippet(key);
    }
}
