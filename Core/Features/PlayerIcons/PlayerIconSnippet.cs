using System;
using ChatPlus.Common.Configs;
using ChatPlus.Core.Features.Stats.Base;
using ChatPlus.Core.Features.Stats.PlayerStats;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.PlayerIcons;

/// <summary>
/// Inline snippet that draws a player's head icon.
/// </summary>
public class PlayerIconSnippet : TextSnippet
{
    private readonly int _playerIndex;
    private readonly bool _isGray;

    public PlayerIconSnippet(int idx, bool isGray = false)
    {
        _playerIndex = idx;
        _isGray = isGray;
        CheckForHover = true;
    }

    // Effect
    private static readonly Lazy<Asset<Effect>> grayscaleEffect =
        new(() => ModContent.Request<Effect>("ChatPlus/Assets/Effects/Grayscale", AssetRequestMode.ImmediateLoad));

    public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch sb,
        Vector2 pos = default, Color color = default, float scale = 1f)
    {
        const float box = 26f;
        scale *= 0.75f;

        size = new Vector2(box * scale + 4f, box * scale);
        if (justCheckingString || color == Color.Black) return true;

        if (_playerIndex < 0 || _playerIndex >= Main.maxPlayers) return true;
        Player player = Main.player[_playerIndex];
        if (player == null || !player.active) return true;

        Vector2 drawPos = new(pos.X + 12f, pos.Y + 8f);

        //sb.End();
        //sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
        //         DepthStencilState.Default, RasterizerState.CullNone,
        //         _isGray ? grayscaleEffect.Value.Value : null,
        //         Main.UIScaleMatrix);

        MapHeadRendererHook.shouldFlipHeadDraw = player.direction == -1;
        Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, player, drawPos, 1f, scale, Color.White);
        MapHeadRendererHook.shouldFlipHeadDraw = false;

        //sb.End();
        //sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        return true;
    }

    public override void OnClick()
    {
        base.OnClick();

        Main.LocalPlayer.mouseInterface = true;

        if (_playerIndex < 0 || _playerIndex >= Main.maxPlayers) return;
        var target = Main.player[_playerIndex];
        if (target == null || !target.active) return;

        // 🔒 block if no access
        if (target != null && !PlayerInfoDrawer.HasAccess(Main.LocalPlayer, target))
        {
            //Main.NewText($"{target.name}'s stats is private.", Color.OrangeRed);
            //return;
        }

        var state = PlayerInfoState.instance;
        if (state == null)
        {
            Main.NewText("Player info UI not available.", Color.Orange);
            return;
        }

        state.SetPlayer(_playerIndex, target.name);          // tell the UI which player to show
        state.OpenForCurrentContext();                 // open the "view more" UI
    }

    public override void OnHover()
    {
        Main.LocalPlayer.mouseInterface = true;

        if (_playerIndex >= 0 && _playerIndex < Main.maxPlayers)
        {
            if (!Conf.C.ShowStatsWhenHovering)
                return;

            if (!Conf.C.ShowStatsWhenBossIsAlive && Main.CurrentFrameFlags.AnyActiveBossNPC)
                return;

            HoveredPlayerOverlay.Set(_playerIndex);
        }
    }
}
