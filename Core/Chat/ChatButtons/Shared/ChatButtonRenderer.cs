using System;
using ChatPlus.Common;
using ChatPlus.Core.Misc;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader;
using static ChatPlus.Common.Configs.Config;

namespace ChatPlus.Core.Chat.ChatButtons.Shared;

public static class ChatButtonRenderer
{
    // Visual sheninagans that only affects the config's visual.
    // Affects nothing functional.
    private static Viewmode p = Viewmode.List;
    public static void TogglePreviewViewmode()
    {
        p = p == Viewmode.List? Viewmode.Grid: Viewmode.List;
    }

    /// <summary>
    /// Main method for drawing chat buttons.
    /// </summary>
    public static void Draw(SpriteBatch sb, ChatButtonType type, Vector2 pos, int size, bool grayscale = false, bool preview = false)
    {
        switch (type)
        {
            case ChatButtonType.Emojis:
                {
                    if (preview)
                    {
                        var tex = GetPreviewEmojiTexture();
                        if (tex != null)
                        {
                            var target = new Rectangle((int)pos.X + 2, (int)pos.Y + 2, size - 4, size - 4);
                            DrawWithGrayscale(sb, grayscale, () => sb.Draw(tex, target, Color.White));
                        }
                    }
                    else
                    {
                        DrawTag(sb, "[e:smiling_face_with_sunglasses]", pos + new Vector2(2, 2), grayscale);
                    }
                    break;
                }

            case ChatButtonType.Uploads:
                {
                    // Use button asset; scale to the requested size so it’s stable at all UIScales
                    DrawTexture(sb, "ChatPlus/Assets/ButtonUpload", pos + new Vector2(0,0), size, grayscale);
                    break;
                }

            case ChatButtonType.Mentions:
                {
                    DrawCharacter(sb, "@", pos + new Vector2(-1, 1), grayscale);
                    break;
                }

            case ChatButtonType.Items:
                {
                    var rect = new Rectangle((int)pos.X + 2, (int)pos.Y + 2, size - 4, size - 4);
                    DrawWithGrayscale(sb, grayscale, () => sb.Draw(TextureAssets.Item[1].Value, rect, Color.White));
                    break;
                }

            case ChatButtonType.Glyphs:
                {
                    float old = GlyphTagHandler.GlyphsScale;
                    Action drawGlyph = () => DrawHelper.DrawText(sb, "[g:0]", pos + new Vector2(2, 2));
                    GlyphTagHandler.GlyphsScale = 0.8f;
                    DrawWithGrayscale(sb, grayscale, drawGlyph);
                    GlyphTagHandler.GlyphsScale = old;
                    break;
                }

            case ChatButtonType.Colors:
                {
                    DrawTexture(sb, "ChatPlus/Assets/ButtonColor", pos, size, grayscale);
                    break;
                }

            case ChatButtonType.Commands:
                {
                    DrawCharacter(sb, "/", pos + new Vector2(3, 3), grayscale);
                    break;
                }

            case ChatButtonType.ModIcons:
                {
                    var tex = Ass.TerrariaIcon?.Value;
                    if (tex != null)
                    {
                        var target = new Rectangle((int)pos.X + 5, (int)pos.Y + 2, size - 9, size - 2);
                        DrawWithGrayscale(sb, grayscale, () => sb.Draw(tex, target, Color.White));
                    }
                    break;
                }

            case ChatButtonType.PlayerIcons:
                {
                    //if (Main.gameMenu || Main.LocalPlayer == null || !Main.LocalPlayer.active || string.IsNullOrEmpty(Main.LocalPlayer.name))
                    //{
                        DrawTexture(sb, "ChatPlus/Assets/AuthorIcon", pos + new Vector2(1, 0), size, grayscale);
                        break;
                    //}

                    //if (preview)
                    //{
                    //    // Stable preview path (no tags in config)
                    //    DrawTexture(sb, "ChatPlus/Assets/AuthorIcon", pos + new Vector2(1, 1), size, grayscale);
                    //}
                    //else
                    //{
                    //    DrawTag(sb, $"[p:{Main.LocalPlayer.name}]", pos + new Vector2(-2f, 1f), grayscale);
                    //}
                    //break;
                }

            case ChatButtonType.Config:
                {
                    DrawTexture(sb, "ChatPlus/Assets/ButtonConfig", pos, size, false);
                    break;
                }

            case ChatButtonType.Viewmode:
                {
                    var openPanel = ChatPlus.StateManager?.GetActivePanel();
                    var gridListTex = ModContent.Request<Texture2D>("ChatPlus/Assets/FilterList", AssetRequestMode.ImmediateLoad).Value;
                    if (gridListTex == null) break;

                    var mode = openPanel != null && !preview
                        ? ChatButtonLayout.GetViewmodeFor(openPanel.GetType())
                        : p; // config uses preview mode

                    var source = mode == Viewmode.List
                        ? new Rectangle(0, 0, 24, 24)
                        : new Rectangle(24, 0, 24, 24);

                    var target = new Rectangle((int)pos.X + 2, (int)pos.Y + 2, size - 4, size - 4);
                    DrawWithGrayscale(sb, false, () => sb.Draw(gridListTex, target, source, Color.White));
                    break;
                }
        }
    }

    private static Texture2D GetPreviewEmojiTexture()
    {
        var asset = ModContent.Request<Texture2D>("ChatPlus/Assets/EmojiBase/1f60e", AssetRequestMode.ImmediateLoad);
        return asset?.Value;
    }

    private static void DrawTag(SpriteBatch sb, string tag, Vector2 pos, bool grayscale)
    {
        string drawTag = grayscale ? tag.Insert(tag.Length - 1, "/gray") : tag;
        DrawHelper.DrawText(sb, drawTag, pos);
    }

    private static void DrawTexture(SpriteBatch sb, string path, Vector2 pos, int size, bool grayscale)
    {
        var tex = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
        if (tex == null) return;

        var target = new Rectangle((int)pos.X + 2, (int)pos.Y + 2, size - 4, size - 4);
        DrawWithGrayscale(sb, grayscale, () => sb.Draw(tex, target, Color.White));
    }

    private static void DrawCharacter(SpriteBatch sb, string text, Vector2 pos, bool grayscale)
    {
        var color = grayscale ? Color.Gray : Color.White;
        DrawHelper.DrawText(sb, text, pos + new Vector2(6f, -3f), color, new Vector2(0.5f), FontAssets.DeathText.Value);
    }

    private static readonly Lazy<Asset<Effect>> grayscaleEffect =
        new(() => ModContent.Request<Effect>("ChatPlus/Assets/Effects/Grayscale", AssetRequestMode.ImmediateLoad));

    private static void DrawWithGrayscale(SpriteBatch sb, bool grayscale, Action drawAction)
    {
        if (!grayscale)
        {
            drawAction();
            return;
        }

        var effectAsset = grayscaleEffect.Value;
        if (effectAsset == null || !effectAsset.IsLoaded || effectAsset.Value == null)
        {
            drawAction();
            return;
        }

        var effect = effectAsset.Value;

        sb.End();
        sb.Begin(
            SpriteSortMode.Immediate,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            Main.Rasterizer,
            effect,
            Main.UIScaleMatrix
        );

        drawAction();

        sb.End();
        sb.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            Main.Rasterizer,
            null,
            Main.UIScaleMatrix
        );
    }
}
