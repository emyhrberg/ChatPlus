using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChatPlus.Common.Debug;
using ChatPlus.Core.Chat.ChatButtons.Shared;
using ChatPlus.Core.Misc;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ChatPlus.Common.Configs.ConfigElements;

public class ChatBoxPreviewElement : ConfigElement
{
    // Blinker state
    private int textBlinkerCount;
    private bool textBlinkerVisible;

    public override void OnBind()
    {
        base.OnBind();
        Width = StyleDimension.Fill;
        HAlign = 0.5f;
        VAlign = 0.5f;
        Label = string.Empty;

        MinHeight.Set(45, 0);
        MaxHeight.Set(45, 0);
        Top.Set(5, 0);
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        // Prevent inventory from being forced open
        if (Main.playerInventory)
        {
            Main.playerInventory = false;
        }
    }

    public override void Draw(SpriteBatch sb)
    {
        MinHeight.Set(45, 0);
        MaxHeight.Set(45, 0);
        Top.Set(5, 0);
        base.Draw(sb);

        DrawChatbox(sb);
        DrawButtons(sb);
    }

    private void DrawBlinker(SpriteBatch sb, Vector2 pos)
    {
        // Update blinker
        textBlinkerCount++;
        if (textBlinkerCount >= 20)
        {
            textBlinkerVisible = !textBlinkerVisible;
            textBlinkerCount = 0;
        }

        // Draw blinker
        if (IsMouseHovering && textBlinkerVisible)
            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, "|", pos + new Vector2(6, 6), Color.White, 0f, Vector2.Zero, Vector2.One);
    }

    private void DrawChatbox(SpriteBatch sb)
    {
        var rect = GetDimensions().ToRectangle();
        int w = rect.Width - 20;
        int h = 32;
        int x = rect.X + 10;
        int y = rect.Y + 6;
        DrawNineSlice(sb, x, y, w, h, TextureAssets.TextBack.Value, new Color(100, 100, 100, 100));
        DrawBlinker(sb, new Vector2(x, y));
    }

    private void DrawNineSlice(SpriteBatch sb, int x, int y, int w, int h, Texture2D tex, Color color)
    {
        int c = 10;
        int ew = tex.Width - c * 2;
        int eh = tex.Height - c * 2;

        sb.Draw(tex, new Vector2(x, y), new Rectangle(0, 0, c, c), color);
        sb.Draw(tex, new Rectangle(x + c, y, w - c * 2, c), new Rectangle(c, 0, ew, c), color);
        sb.Draw(tex, new Vector2(x + w - c, y), new Rectangle(tex.Width - c, 0, c, c), color);

        sb.Draw(tex, new Rectangle(x, y + c, c, h - c * 2), new Rectangle(0, c, c, eh), color);
        sb.Draw(tex, new Rectangle(x + c, y + c, w - c * 2, h - c * 2), new Rectangle(c, c, ew, eh), color);
        sb.Draw(tex, new Rectangle(x + w - c, y + c, c, h - c * 2), new Rectangle(tex.Width - c, c, c, eh), color);

        sb.Draw(tex, new Vector2(x, y + h - c), new Rectangle(0, tex.Height - c, c, c), color);
        sb.Draw(tex, new Rectangle(x + c, y + h - c, w - c * 2, c), new Rectangle(c, tex.Height - c, ew, c), color);
        sb.Draw(tex, new Vector2(x + w - c, y + h - c), new Rectangle(tex.Width - c, tex.Height - c, c, c), color);
    }

    private void DrawButtons(SpriteBatch sb)
    {
        List<ChatButtonType> types = [];
        foreach (var type in ChatButtonLayout.OrderRightToLeft)
        {
            if (IsEnabledLive(type))
                types.Add(type);
        }
        if (types.Count == 0) return;

        var dims = GetDimensions();
        int size = 24;
        float gap = 2f;

        float baseX = dims.X + dims.Width - size - 20f;
        float y = dims.Y + dims.Height - size - 11f;

        bool anyHover = false;
        var mouse = Main.MouseScreen.ToPoint(); // UI-scaled mouse pos

        for (int i = 0; i < types.Count; i++)
        {
            var type = types[i];
            gap = (type == ChatButtonType.Config || type == ChatButtonType.Viewmode) ? 2 : 2; // same for now
            float x = baseX - i * (size + gap);
            var pos = new Vector2(x, y);
            var hit = new Rectangle((int)x, (int)y, size, size);

            bool hovered = hit.Contains(mouse);
            anyHover |= hovered;

            ChatButtonRenderer.Draw(sb, types[i], pos, size, grayscale: !hovered, preview: true);

            // draw border and tooltip around hovered item
            if (hovered)
            {
                var r = hit;
                r.Inflate(2, 2);
                DrawHelper.DrawPixelatedBorder(sb, r, Color.Gray, 2, 2);

                string tt = type.ToString();

                if (type == ChatButtonType.PlayerIcons) tt = "Player Icons";
                if (type == ChatButtonType.ModIcons) tt = "Mod Icons";

                UICommon.TooltipMouseText(tt);
            }

            if (hovered && Main.mouseLeft && Main.mouseLeftRelease && type == ChatButtonType.Viewmode)
            {
                // don't do this, it's confusing for users
                //ChatButtonRenderer.TogglePreviewViewmode();
            }
        }

        // restart sb with UIScaleMatrix (doesnt fix anything)
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

    private bool IsEnabledLive(ChatButtonType type)
    {
        if (Item == null)
        {
            return true;
        }

        string fieldName = type switch
        {
            ChatButtonType.Commands => "ShowCommandButton",
            ChatButtonType.Colors => "ShowColorButton",
            ChatButtonType.Emojis => "ShowEmojiButton",
            ChatButtonType.Glyphs => "ShowGlyphButton",
            ChatButtonType.Items => "ShowItemButton",
            ChatButtonType.ModIcons => "ShowModIconButton",
            ChatButtonType.Mentions => "ShowMentionButton",
            ChatButtonType.PlayerIcons => "ShowPlayerIconButton",
            ChatButtonType.Uploads => "ShowUploadButton",
            ChatButtonType.Config => "ShowConfigButton",
            ChatButtonType.Viewmode => "ShowViewmodeButton",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(fieldName))
        {
            return true;
        }

        var itemType = Item.GetType();

        var field = itemType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            var value = field.GetValue(Item);
            if (value is bool b1)
            {
                if (value is null) Log.Error("config item type field value is null");
                return b1;
            }
        }

        return true;
    }
}
