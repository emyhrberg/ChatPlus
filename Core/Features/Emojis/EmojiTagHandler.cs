using System;
using System.Collections.Generic;
using ChatPlus.Common.Debug;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Emojis
{
    public class EmojiTagHandler : ITagHandler
    {
        /// <summary>
        /// A dictionary to hold emoji keys and their corresponding uploadedTexture assets.
        /// </summary>
        private static readonly Dictionary<string, Asset<Texture2D>> Registry = new(StringComparer.OrdinalIgnoreCase);

        public static bool RegisterEmoji(string key, string texturePath)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(texturePath))
                return false;
            try
            {
                var asset = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.AsyncLoad);
                Registry[key] = asset;
                return true;
            }
            catch
            {
                Log.Error($"Failed to find texturePath: '{texturePath}' for key: '{key}'");
                return false;
            }
        }

        TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
        {
            // Handle optional "/gray" suffix
            bool gray = false;
            string key = text;

            int slashIndex = text.IndexOf('/');
            if (slashIndex >= 0)
            {
                string suffix = text[(slashIndex + 1)..].Trim().ToLowerInvariant();
                key = text[..slashIndex];
                if (suffix == "gray" || suffix == "grayscale")
                    gray = true;
            }

            if (!Registry.TryGetValue(key, out var texture))
            {
                return new TextSnippet(text);
            }

            string tag = GenerateEmojiTag(text);
            EmojiSnippet snippet = new(texture, tag, gray);
            return snippet;
        }

        public static string GenerateEmojiTag(string key)
        {
            return "[e:" + key + "]";
        }
    }
}