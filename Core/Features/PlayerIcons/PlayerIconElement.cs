using System;
using ChatPlus.Common.Configs;
using ChatPlus.Core.Features.PlayerColors;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;
using static ChatPlus.Common.Configs.ClientConfig;

namespace ChatPlus.Core.Features.PlayerIcons;

public class PlayerIconElement : BaseElement<PlayerIcon>
{
    public PlayerIconElement(PlayerIcon data) : base(data)
    {
        Height.Set(30, 0);
        Width.Set(0, 1);
    }

    protected override void DrawGridElement(SpriteBatch sb)
    {
        var dims = GetDimensions();
        Vector2 pos = dims.Position();
        TextSnippet[] snip = [new TextSnippet(Data.PlayerName)];
        string tag = Data.Tag;

        // Draw player head
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, tag,
            pos + new Vector2(0, 3), Color.White, 0f, Vector2.Zero, new Vector2(1.05f), -1f, 1f);
    }

    protected override void DrawListElement(SpriteBatch sb)
    {
        var dims = GetDimensions();
        Vector2 pos = dims.Position();
        TextSnippet[] snip = [new TextSnippet(Data.PlayerName)];
        string tag = Data.Tag;

        // Draw player head
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, tag,
            pos + new Vector2(4, 3), Color.White, 0f, Vector2.Zero, new Vector2(1.05f), -1f, 1f);

        // Draw outline
        pos += new Vector2(35, 4);
        ChatManager.DrawColorCodedStringShadow(sb, FontAssets.MouseText.Value, snip, pos, Color.Black, 0f, Vector2.Zero, Vector2.One);

        // Draw player name in synced color
        string hex = "FFFFFF";
        if (PlayerColorSystem.PlayerColors.TryGetValue(Data.PlayerIndex, out var syncedHex))
            hex = syncedHex ?? "FFFFFF";

        var coloredSnips = ChatManager.ParseMessage($"[c/{hex}:{Data.PlayerName}]", Color.White).ToArray();
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, coloredSnips,
            pos, 0f, Vector2.Zero, Vector2.One, out _, 500f);
    }
}
