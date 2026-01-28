using System;
using System.Linq;
using System.Text.RegularExpressions;
using ChatPlus.Common.Configs;
using ChatPlus.Core.Features.Scrollbar;
using ChatPlus.Core.Features.Stats.Base;
using ChatPlus.Core.Features.TypingIndicators;
using ChatPlus.Core.Features.Uploads;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Chat;

/// <summary>
/// Draws the entire chat
/// Including the chatbox background, input line, caret, selection rectangle, upload preview, and the chat messages.
/// </summary>
internal class DrawChatSystem : ModSystem
{
    public override void Load()
    {
        if (ModLoader.TryGetMod("ChatImprover", out Mod _))
        {
            return;
        }
        On_Main.DrawPlayerChat += DrawChat;
        On_Main.DrawPlayerChat += DrawUIInFullscreenMap;
        On_Main.DrawPendingMouseText += DrawTopMostUIInFullscreenMap;
        On_RemadeChatMonitor.DrawChat += DrawMonitor;
        On_Main.DrawMap += PreventMapDrag;
        On_Player.Update += PreventMapZoom;
    }

    public override void Unload()
    {
        if (ModLoader.TryGetMod("ChatImprover", out Mod _))
        {
            return;
        }

        On_Main.DrawPlayerChat -= DrawChat;
        On_Main.DrawPlayerChat -= DrawUIInFullscreenMap;
        On_Main.DrawPendingMouseText -= DrawTopMostUIInFullscreenMap;
        On_RemadeChatMonitor.DrawChat -= DrawMonitor;
        On_Main.DrawMap -= PreventMapDrag;
        On_Player.Update -= PreventMapZoom;
    }
    #region Fullscreen Map Fixes
    private void PreventMapZoom(On_Player.orig_Update orig, Player self, int i)
    {
        if (i != Main.myPlayer || !Main.mapFullscreen)
        {
            orig(self, i);
            return;
        }

        bool blockZoom = ShouldBlockMapInput();
        //Log.Debug("blockZoom: " + blockZoom);

        // snapshot
        int wheel = PlayerInput.ScrollWheelDelta;
        bool zoomIn = self.mapZoomIn;
        bool zoomOut = self.mapZoomOut;

        try
        {
            if (blockZoom)
            {
                PlayerInput.ScrollWheelDelta = 0;
                self.mapZoomIn = false;
                self.mapZoomOut = false;
            }
            orig(self, i);
        }
        finally
        {
            PlayerInput.ScrollWheelDelta = wheel;
            self.mapZoomIn = zoomIn;
            self.mapZoomOut = zoomOut;
        }
    }

    private void PreventMapDrag(On_Main.orig_DrawMap orig, Main self, GameTime gameTime)
    {
        if (!Main.mapFullscreen)
        {
            orig(self, gameTime);
            return;
        }

        bool blockDrag = ShouldBlockMapInput();
        //Log.Debug("blockDrag: " + blockDrag);

        // snapshot
        bool oldLeft = Main.mouseLeft;
        bool oldMI = Main.LocalPlayer.mouseInterface;

        try
        {
            if (blockDrag)
            {
                Main.LocalPlayer.mouseInterface = true;
                Main.mouseLeft = false; // prevent pan
                PlayerInput.LockVanillaMouseScroll("ChatPlus/MapBlockScroll"); // prevent zoom
            }

            orig(self, gameTime);
        }
        finally
        {
            Main.mouseLeft = oldLeft;
            Main.LocalPlayer.mouseInterface = oldMI;

            if (blockDrag)
            {
                Main.grabMapX = Main.mouseX;
                Main.grabMapY = Main.mouseY;
            }
        }
    }

    private static bool ShouldBlockMapInput()
    {
        if (!Main.mapFullscreen)
            return false;

        // check info states
        bool overInfo = Main.InGameUI?.CurrentState is BaseInfoState s && s.IsMouseOverRoot();

        // draggable panels
        bool overPanel = DraggablePanel.AnyHovering;

        // scrollbars
        bool overScroll = DraggablePanel.IsAnyScrollbarHovering();

        // chat history bits
        var chat = ModContent.GetInstance<ChatScrollSystem>()?.chatScrollState;
        bool overChatScrollbar = (chat?.chatScrollbar?.IsMouseHovering ?? false)
            || (chat?.chatScrollList.IsMouseHovering ?? false);

        return overInfo || overPanel || overScroll || overChatScrollbar;
    }
    private void DrawUIInFullscreenMap(On_Main.orig_DrawPlayerChat orig, Main self)
    {
        orig(self);

        if (Main.mapFullscreen)
        {
            DrawSystemsInFullscreenMap.Draw();
            DrawSystemsInFullscreenMap.DrawInfoStatesTopMost();
        }
    }

    private void DrawTopMostUIInFullscreenMap(On_Main.orig_DrawPendingMouseText orig)
    {
        orig();

        if (Main.mapFullscreen)
        {
            DrawSystemsInFullscreenMap.DrawHoverInfoSystems();
        }
    }
    #endregion
    private void DrawMonitor(On_RemadeChatMonitor.orig_DrawChat orig, RemadeChatMonitor self, bool drawingPlayerChat)
    {
        int oldH = Main.screenHeight;
        int delta = 0;

        try
        {
            // Upload adjustment
            if (drawingPlayerChat && UploadTagHandler.ContainsUploadTag(Main.chatText))
            {
                float ui = Main.UIScaleMatrix.M11;
                if (ui <= 0f) ui = 1f;
                delta += (int)Math.Round(147f / ui);
            }

            // Collect other typers (exclude self)
            var typingPlayers = TypingIndicatorSystem.TypingPlayers
                .Where(kvp =>
                    kvp.Value &&
                    kvp.Key >= 0 && kvp.Key < Main.maxPlayers &&
                    Main.player[kvp.Key].active)
                .Select(kvp => Main.player[kvp.Key].name)
                .ToList();

            // to debug, comment below line out
            typingPlayers.Remove(Main.player[Main.myPlayer].name);

            delta += 30;

            if (!Conf.C.TypingIndicators)
                delta -= 10;

            bool anyTyping = typingPlayers.Any();

            if (delta > 0)
                Main.screenHeight = Math.Max(0, oldH - delta);

            // Draw chat history
            orig(self, drawingPlayerChat);
            // Draw typing line
            if (anyTyping)
            {
                TypingIndicatorSystem.DrawTypingLine(yOffset: 38);
            }
        }
        finally
        {
            Main.screenHeight = oldH;
        }
    }
    private void DrawChat(On_Main.orig_DrawPlayerChat orig, Main self)
    {
        if (!Conf.C.TextEditor)
        {
            orig(self);
            return;
        }

        if (!Main.drawingPlayerChat)
        {
            orig(self);
            return;
        }

        PlayerInput.WritingText = true;
        Main.instance.HandleIME();
        if (++Main.instance.textBlinkerCount >= 20)
        {
            Main.instance.textBlinkerState = 1 - Main.instance.textBlinkerState;
            Main.instance.textBlinkerCount = 0;
        }

        // Determine height
        bool hasUpload = UploadTagHandler.ContainsUploadTag(Main.chatText);
        int height = 32;
        if (hasUpload)
            height = 147 + 21;

        int x = 0;
        int y = -27;

        if (!Conf.C.TypingIndicators)
            y += 10;

        DrawChatbox(height, x, y);

        if (!hasUpload)
        {
            DrawInputLine(height, hasUpload, x, y);
            DrawSelectionRectangle(height, x, y);
        }
        else
        {
            // reserve horizontal space for the upload preview and shift both text & caret
            int textOffsetX = DrawUploadAndGetTextOffset(height - y - 5);
            DrawInputLine(height, hasUpload, textOffsetX, y);
            DrawSelectionRectangle(height, textOffsetX, y);
        }

        Main.chatMonitor.DrawChat(true);
    }

    private static int DrawUploadAndGetTextOffset(int totalHeight)
    {
        if (!TryGetFirstUploadTexture(Main.chatText, out var tex)) return 0;

        int inputX = 88;
        int inputY = Main.screenHeight - totalHeight;

        float targetH = 147;
        float s = targetH / tex.Height;
        float drawnW = tex.Width * s;

        Main.spriteBatch.Draw(tex, new Vector2(inputX, inputY), null, Color.White, 0f, Vector2.Zero, s, SpriteEffects.None, 0f);
        return (int)Math.Ceiling(drawnW) + 8;
    }

    private static bool TryGetFirstUploadTexture(string s, out Texture2D tex)
    {
        tex = null;
        var m = Regex.Match(s ?? "", @"\[u:([^\]]+)\]", RegexOptions.IgnoreCase);
        if (!m.Success) return false;
        string key = m.Groups[1].Value.Trim();
        return UploadTagHandler.TryGet(key, out tex);
    }

    private static void DrawChatbox(int height, int xOffset = 0, int yOffset = 0)
    {
        int w = Main.screenWidth - 300;
        int y = Main.screenHeight - 4 - height + yOffset;
        int x = 78 + xOffset;
        DrawNineSlice(x, y, w, height, TextureAssets.TextBack.Value, new Color(100, 100, 100, 100));
    }
    private static readonly Regex RxUpload = new(@"(?i)\[u:[^\]]+\]");
    private static readonly Regex RxColorPx = new(@"(?i)\[c/[^:\]]*:");

    private static void DrawInputLine(int height, bool hasUpload, int xOffset = 0, int yOffset = 0)
    {
        int baseX = 88;
        int baseY = Main.screenHeight - height;

        // 1) Raw text from chat
        string raw = Main.chatText ?? string.Empty;

        // 2) Remove upload tag from the DRAWN text (we already render the image beside)
        Match u = RxUpload.Match(raw);
        string renderText = u.Success ? raw.Remove(u.Index, u.Length) : raw;

        // 3) Map raw-caret -> visible-caret (subtract hidden segments)
        int caretRaw = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, raw.Length);
        int caretVis = caretRaw;

        // 3a) adjust for upload tag
        if (u.Success)
        {
            int uStart = u.Index;
            int uEnd = u.Index + u.Length;
            if (caretRaw > uStart)
            {
                if (caretRaw < uEnd) caretVis = uStart;          // caret inside tag → snap to tag start (visible = 0 shift here)
                else caretVis -= u.Length;                        // caret after tag → shift left by tag length
            }
        }

        // 3b) adjust for ALL color prefixes before caret and their closing bracket if present
        for (Match m = RxColorPx.Match(raw); m.Success && m.Index < caretRaw; m = m.NextMatch())
        {
            int pxStart = m.Index;
            int pxEnd = m.Index + m.Length;                       // length of "[c/...:"
                                                                  // subtract prefix length if caret is beyond the prefix
            caretVis -= Math.Max(0, Math.Min(caretRaw, pxEnd) - pxStart);

            // if the closing ']' for this color tag exists and is before the caret, subtract that too
            int close = raw.IndexOf(']', pxEnd);
            if (close != -1 && close < caretRaw) caretVis -= 1;

            // If caret is inside the prefix itself, keep it at the prefix start (so it doesn't drift)
            if (caretRaw >= pxStart && caretRaw < pxEnd)
                caretVis = Math.Min(caretVis, pxStart);
        }

        // Safety clamp to the text we actually draw
        caretVis = Math.Clamp(caretVis, 0, renderText.Length);

        // 4) Draw the line using the text that still contains color tags (so ChatManager colors it)
        var snips = ChatManager.ParseMessage(renderText, Color.White).ToArray();
        ChatManager.DrawColorCodedStringWithShadow(
            Main.spriteBatch,
            FontAssets.MouseText.Value,
            snips,
            new Vector2(baseX + xOffset, baseY + yOffset),
            0f,
            Vector2.Zero,
            Vector2.One,
            out _
        );

        // 5) Draw the caret at the visible position
        if (Main.instance.textBlinkerState == 1)
        {
            var before = ChatManager.ParseMessage(renderText.Substring(0, caretVis), Color.White).ToArray();
            Vector2 beforeSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, before, Vector2.One);
            int caretX = baseX + xOffset + (int)beforeSize.X;
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(caretX, baseY + 2 + yOffset, 1, 17), Color.White);
        }
    }

    private void DrawSelectionRectangle(int height, int xOffset = 0, int yOffset = 0)
    {
        var sel = HandleChatSystem.GetSelection();
        if (sel == null) return;

        string raw = Main.chatText ?? "";
        var tag = Regex.Match(raw, @"\[u:[^\]]+\]", RegexOptions.IgnoreCase);

        int start = sel.Value.start;
        int end = sel.Value.end;

        if (tag.Success)
        {
            int tagStart = tag.Index;
            int tagEnd = tag.Index + tag.Length;

            if (end <= tagStart)
            {
                start = 0;
                end = 0;
            }
            else if (start < tagEnd && end > tagStart)
            {
                start = tagEnd;
            }

            if (start < tagEnd) start = tagEnd;
            if (end < start) end = start;
        }

        string cleaned = tag.Success ? raw.Remove(tag.Index, tag.Length) : raw;

        int adjStart = tag.Success ? System.Math.Max(0, start - tag.Length) : start;
        int adjEnd = tag.Success ? System.Math.Max(0, end - tag.Length) : end;

        adjStart = System.Math.Clamp(adjStart, 0, cleaned.Length);
        adjEnd = System.Math.Clamp(adjEnd, 0, cleaned.Length);
        if (adjStart >= adjEnd) return;

        Vector2 preSize = MeasureSnippets(cleaned.Substring(0, adjStart));
        Vector2 midSize = MeasureSnippets(cleaned.Substring(adjStart, adjEnd - adjStart));

        var rect = new Rectangle(
            88 + xOffset + (int)System.Math.Floor(preSize.X),
            Main.screenHeight - height + yOffset,
            (int)System.Math.Ceiling(midSize.X),
            20
        );

        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, new Color(17, 85, 204) * 0.5f);

        static Vector2 MeasureSnippets(string s)
        {
            var snips = ChatManager.ParseMessage(s ?? "", Color.White).ToArray();
            return ChatManager.GetStringSize(FontAssets.MouseText.Value, snips, Vector2.One);
        }
    }

    private static void DrawNineSlice(int x, int y, int w, int h,Texture2D tex, Color color)
    {
        int c = 10;
        int ew = tex.Width - c * 2;
        int eh = tex.Height - c * 2;

        var sb = Main.spriteBatch;
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

    private static void DrawDebugRect(Rectangle r) => Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, r, Color.Red * 0.5f);
}
