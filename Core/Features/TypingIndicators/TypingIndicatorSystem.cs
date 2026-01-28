using ChatPlus.Common;
using ChatPlus.Common.Configs;
using ChatPlus.Core.Features.PlayerColors;
using ChatPlus.Core.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.TypingIndicators;

[Autoload(Side =ModSide.Both)]
public class TypingIndicatorSystem : ModSystem
{
    public static Dictionary<int, bool> TypingPlayers = [];

    // the fade value goes from 0 to 6 (6 frames to fully appear, 6 frames to fully disappear)
    private static readonly int[] fade = new int[Main.maxPlayers];

    /// Draws the chat bubble above the players
    public override void PostDrawInterface(SpriteBatch sb)
    {
        if (Conf.C.TypingIndicators == Config.Privacy.NoOne)
        {
            return;
        }

        if (Main.gameMenu) return;

        Texture2D bubbleTex = TextureAssets.Extra[48].Value;
        Texture2D typingTex = Ass.TypingIndicatorDotsOnly.Value;

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];
            if (!p.active)
            {
                fade[i] = 0;
                TypingPlayers.Remove(i);
                continue;
            }

            bool typing = TypingPlayers.TryGetValue(i, out var v) && v && CanSeeTyping(Main.myPlayer, i);

            if (typing && fade[i] == 0)
            {
                foreach (var kv in EmoteBubble.byID)
                {
                    var b = kv.Value;
                    if (b.anchor.type == WorldUIAnchor.AnchorType.Entity && b.anchor.entity == p)
                    {
                        b.lifeTime = 6;
                    }
                }
            }

            if (typing)
            {
                if (fade[i] < 6) fade[i]++;
            }
            else
            {
                if (fade[i] > 0) fade[i]--;
            }

            if (fade[i] == 0) continue;

            // debug pos, works in SP, not in MP.
            Vector2 goodPos = new(p.Top.X, p.VisualPosition.Y - 7f);
            Vector2 side = new(-p.direction * p.width * 0.75f, 2f);
            goodPos += side;
            goodPos -= Main.screenPosition;
            goodPos = goodPos.Floor();
            //Log.Info("goodPos: " + goodPos);

            // World anchor (same spot vanilla emotes use)
            Vector2 world = new Vector2(p.Top.X, p.VisualPosition.Y - 7f)
              + new Vector2(-p.direction * p.width * 0.75f, 2f);

            Matrix toUi = Main.GameViewMatrix.TransformationMatrix * Matrix.Invert(Main.UIScaleMatrix);
            Vector2 pos = Vector2.Transform(world - Main.screenPosition, toUi).Floor();
            //Log.Info("worldPos: " + pos);

            SpriteEffects fx = SpriteEffects.None;
            if (p.direction != -1)
            {
                fx = SpriteEffects.FlipHorizontally;
            }

            bool small = fade[i] < 6;
            int sheetColumn = 0;
            if (!small)
            {
                sheetColumn = 1;
            }

            Rectangle bubbleFrame = bubbleTex.Frame(8, EmoteBubble.EMOTE_SHEET_VERTICAL_FRAMES, sheetColumn, 0);
            Vector2 origin = new(bubbleFrame.Width / 2f, bubbleFrame.Height);

            sb.Draw(bubbleTex, pos, bubbleFrame, Color.White, 0f, origin, 1f, fx, 0f);

            if (fade[i] > 4)
            {
                Rectangle src = GetTypingSourceRect();

                // Do not flip the dots texture; mirror the offset instead.
                Vector2 offset = new Vector2(-16f, -28f);
                const float dotsScale = 1f;

                if ((fx & SpriteEffects.FlipHorizontally) != 0)
                {
                    offset.X = -offset.X + 1f - src.Width * dotsScale;
                }
                if ((fx & SpriteEffects.FlipVertically) != 0)
                {
                    offset.Y = -offset.Y - src.Height * dotsScale;
                }

                sb.Draw(typingTex, pos + offset, src, Color.White, 0f, Vector2.Zero, dotsScale, SpriteEffects.None, 0f);
            }
        }
    }

    /// Draws the chat bubble just below the chat box
    public static void DrawTypingLine(int yOffset=0)
    {
        if (Conf.C.TypingIndicators == Config.Privacy.NoOne) return;

        // Show chatline ONLY for other players (never for myself).
        var otherTypers = TypingPlayers
            .Where(kvp => kvp.Value
                         && kvp.Key >= 0
                         && kvp.Key < Main.maxPlayers
                         && Main.player[kvp.Key].active
                         && CanSeeTyping(Main.myPlayer, kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();

        // remove myself if present
        otherTypers.Remove(Main.myPlayer);

        if (otherTypers.Count == 0) return;

        List<string> coloredNames = [];
        for (int n = 0; n < otherTypers.Count; n++)
        {
            int idx = otherTypers[n];
            Player p = Main.player[idx];
            string name = p.name;

            string hex;
            if (PlayerColorSystem.PlayerColors.TryGetValue(idx, out var syncedHex) && !string.IsNullOrWhiteSpace(syncedHex))
            {
                hex = syncedHex;
            }
            else
            {
                hex = PlayerColorHandler.HexFromName(name);
            }

            //coloredNames.Add($"[c/{hex}:{name}]");
            coloredNames.Add(name);
        }

        string message;
        if (coloredNames.Count == 1)
        {
            message = Loc.Get("TypingIndicators.IsTyping", coloredNames[0]);
        }
        else if (coloredNames.Count <= 3)
        {
            message = Loc.Get("TypingIndicators.FewPlayersAreTyping", string.Join(", ", coloredNames));
        }
        else
        {
            message = Loc.Get("TypingIndicators.MultiplePlayersAreTyping", coloredNames.Count);
        }

        Vector2 pos = new(65, Main.screenHeight - 38 + yOffset);

        Texture2D tex = Ass.TypingIndicator.Value;
        Rectangle src = GetTypingSourceRect();
        Vector2 innerPos = pos + new Vector2(4, 4);
        Main.spriteBatch.Draw(tex, innerPos, src, Color.White, 0f, Vector2.Zero, 0.73f, 0f, 0f);

        var snippets = ChatManager.ParseMessage(message, Color.Gray).ToArray();
        ChatManager.DrawColorCodedStringWithShadow(
            Main.spriteBatch,
            FontAssets.MouseText.Value,
            snippets,
            pos + new Vector2(30, 3),
            0f,
            Vector2.Zero,
            new Vector2(0.9f),
            out _
        );
    }

    public static Rectangle GetTypingSourceRect()
    {
        int speed = 8; // <5 is fast, 5-8 = medium, >8 is slow
        int frameCount = 8; 
        int w = 32;
        int h = 26;
        int frame = (int)((Main.GameUpdateCount / speed) % frameCount);
        return new Rectangle(frame * 32, 0, w, h);
    }

    internal static bool CanSeeTyping(int viewerWho, int typerWho)
    {
        var mode = Conf.C.TypingIndicators;

        if (mode == Config.Privacy.NoOne)
            return false;

        if (viewerWho == typerWho)
            return false;

        if (mode == Config.Privacy.Everyone)
            return true;

        // Team
        if (mode == Config.Privacy.Team)
        {
            var v = Main.player[viewerWho];
            var t = Main.player[typerWho];

            if (!v.active || !t.active)
                return false;

            if (v.team == 0)
                return false;

            return v.team == t.team;
        }

        return false;
    }
}
