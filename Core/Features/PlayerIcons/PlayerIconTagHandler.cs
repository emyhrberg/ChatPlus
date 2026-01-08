using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.PlayerIcons;

/// <summary>
/// Parses [p:PlayerName] into a <see cref="PlayerIconSnippet"/> showing the player's head.
/// </summary>
public sealed class PlayerIconTagHandler : ITagHandler
{
    public static string GenerateTag(string name) => PlayerIconManager.GenerateTag(name);

    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
    {
        // Handle optional "/gray" suffix
        bool gray = false;
        string name = (text ?? string.Empty).Trim();


        int slashIndex = name.IndexOf('/');
        if (slashIndex >= 0)
        {
            string suffix = name[(slashIndex + 1)..].Trim().ToLowerInvariant();
            name = name[..slashIndex];
            if (suffix == "gray" || suffix == "grayscale")
                gray = true;
        }

        if (!PlayerIconManager.TryGetIndex(name, out int idx))
        {
            // try case-insensitive match over current players
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var plr = Main.player[i];
                if (plr?.active == true && string.Equals(plr.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    idx = i;
                    break;
                }
            }
        }

        if (idx < 0 || idx >= Main.maxPlayers)
            return new TextSnippet(name);

        return new PlayerIconSnippet(idx, gray)
        {
            Text = GenerateTag(name + (gray ? "/gray" : string.Empty))
        };
    }
}
