using System;
using ChatPlus.Common.Configs;
using ChatPlus.Core.Features.Glyphs;
using ChatPlus.Core.Features.PlayerColors;
using ChatPlus.Core.Features.PlayerIcons;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;
using static ChatPlus.Common.Configs.ClientConfig;

namespace ChatPlus.Core.Features.Mentions;

public class MentionElement : BaseElement<Mention>
{
    public MentionElement(Mention data) : base(data)
    {
        Height.Set(30, 0);
        Width.Set(0, 1);
    }
    protected override void DrawGridElement(SpriteBatch sb)
    {
        var dims = GetDimensions();
        Vector2 pos = dims.Position();
        string playerName = Data.Tag;

        // Player head
        string headTag = PlayerIconTagHandler.GenerateTag(playerName);
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, headTag,
            pos + new Vector2(0, 3), Color.White, 0f, Vector2.Zero, new Vector2(1.05f), -1f, 1f);
    }

    protected override void DrawListElement(SpriteBatch sb)
    {
        var dims = GetDimensions();
        Vector2 pos = dims.Position();
        string playerName = Data.Tag;

        // Player head
        string headTag = PlayerIconTagHandler.GenerateTag(playerName);
        ChatManager.DrawColorCodedStringWithShadow(
            sb, FontAssets.MouseText.Value, headTag,
            pos + new Vector2(4, 3), Color.White, 0f, Vector2.Zero, new Vector2(1.05f), -1f, 1f
        );

        // Colorized player name (default white unless a synced color exists)
        pos += new Vector2(35, 4);
        string hex = "FFFFFF";

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var p = Main.player[i];
            if (p?.active == true && string.Equals(p.name, playerName, StringComparison.Ordinal))
            {
                if (PlayerColorSystem.PlayerColors.TryGetValue(i, out var syncedHex) &&
                    !string.IsNullOrWhiteSpace(syncedHex))
                {
                    hex = syncedHex;
                }
                break;
            }
        }

        var coloredSnips = ChatManager.ParseMessage($"[c/{hex}:{playerName}]", Color.White).ToArray();
        ChatManager.DrawColorCodedStringWithShadow(
            sb, FontAssets.MouseText.Value, coloredSnips,
            pos, 0f, Vector2.Zero, Vector2.One, out _, 500f
        );
    }
}
