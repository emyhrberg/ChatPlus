using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using ChatPlus.Common.Debug;
using ChatPlus.Core.Features.Links;
using ChatPlus.Core.Features.Mentions;
using ChatPlus.Core.Features.Uploads;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatPlus.Common.Compat;

/// <summary>
/// Does 3 things:
/// 1. Adds extra lines for upload snippets.
/// 2. Detects link snippets.
/// 3. Moves the chat monitor x lines upwards to avoid chat monitor colliding with images.
/// </summary>
internal class ChitterChatterSystem : ModSystem
{
    private Hook newTextHook;
    private Hook renderHook;

    private delegate void newTextOrig(object self, string text, bool force, Color c, int widthLimit);
    private delegate void RenderChatOrig(object self, bool extendedChatWindow);

    public override void Load()
    {
        if (ModLoader.TryGetMod("ChitterChatter", out Mod cc))
        {
            InitializeNewTextHook(cc);
            InitializeRenderChatHook(cc);
        }
    }

    public override void Unload()
    {
        newTextHook?.Dispose();
        newTextHook = null;
        renderHook?.Dispose();
        renderHook = null;
    }

    private void InitializeRenderChatHook(Mod cc)
    {
        var type = cc.Code.GetType("Tomat.TML.Mod.ChitterChatter.Content.Features.ChatMonitor.Rooms.VanillaChatRoom");
        var method = type.GetMethod("RenderChat", BindingFlags.Public | BindingFlags.Instance);

        renderHook = new Hook(
            method,
            new Action<RenderChatOrig, object, bool>((orig, self, extendedChatWindow) =>
            {
                // Re-implementation of VanillaChatRoom.RenderChat with pad-line skip
                var messages = (System.Collections.IList)self.GetType().GetField("messages", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(self);
                int messagesToShow = (int)self.GetType().GetField("messagesToShow", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(self);
                int startMessageIdx = (int)self.GetType().GetField("startMessageIdx", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(self);

                int remainingLines = startMessageIdx;
                int messageIndex = 0;
                int lineOffset = 0;

                // Walk to starting position
                while (remainingLines > 0 && messageIndex < messages.Count)
                {
                    var msg = messages[messageIndex];
                    int lineCount = (int)msg.GetType().GetProperty("LineCount", BindingFlags.Public | BindingFlags.Instance)!.GetValue(msg)!;
                    int toProcess = Math.Min(remainingLines, lineCount);
                    remainingLines -= toProcess;
                    lineOffset = toProcess;

                    if (lineOffset == lineCount)
                    {
                        lineOffset = 0;
                        messageIndex++;
                    }
                }

                int displayedLines = 0;
                int? hoveredMessageIndex = null;
                int snippetIndex = -1;
                int? hoveredSnippetIndex = null;

                while (displayedLines < messagesToShow && messageIndex < messages.Count)
                {
                    var msg = messages[messageIndex];

                    bool prepared = (bool)msg.GetType().GetProperty("Prepared", BindingFlags.Public | BindingFlags.Instance)!.GetValue(msg)!;
                    bool canShow = (bool)msg.GetType().GetProperty("CanBeShownWhenChatIsClosed", BindingFlags.Public | BindingFlags.Instance)!.GetValue(msg)!;
                    if (!prepared || !(extendedChatWindow || canShow)) break;

                    // snippets for this visual line
                    var getSnip = msg.GetType().GetMethod("GetSnippetWithInversedIndex", BindingFlags.Public | BindingFlags.Instance)!;
                    var snippets = (TextSnippet[])getSnip.Invoke(msg, new object[] { lineOffset })!;

                    // If this line is our padding → count it (spacing), but skip timestamp + content draw
                    if (snippets.Length == 1 && snippets[0] is PadSnippet)
                    {
                        displayedLines++;
                        lineOffset++;

                        int lineCount = (int)msg.GetType().GetProperty("LineCount", BindingFlags.Public | BindingFlags.Instance)!.GetValue(msg)!;
                        if (lineOffset >= lineCount)
                        {
                            lineOffset = 0;
                            messageIndex++;
                        }
                        continue;
                    }

                    // Normal line draw (timestamp + content)
                    int yPos = Main.screenHeight - 30 - 28 - displayedLines * 21;
                    var font = FontAssets.MouseText.Value;

                    // timestamp
                    {
                        var utc = (DateTime)msg.GetType().GetProperty("UtcTime", BindingFlags.Public | BindingFlags.Instance)!.GetValue(msg)!;
                        string timestamp = utc.ToLocalTime().ToShortTimeString();
                        float tsWidth = font.MeasureString(timestamp).X;

                        ChatManager.DrawColorCodedStringWithShadow(
                            Main.spriteBatch, font, timestamp,
                            new Vector2(88f - tsWidth - 4f, yPos),
                            Color.DarkGray, 0f, Vector2.Zero, Vector2.One
                        );
                    }

                    // content
                    int hoveredSnippet;
                    ChatManager.DrawColorCodedStringWithShadow(
                        Main.spriteBatch, font, snippets,
                        new Vector2(88f, yPos),
                        0f, Vector2.Zero, Vector2.One,
                        out hoveredSnippet
                    );

                    if (hoveredSnippet >= 0)
                    {
                        hoveredSnippetIndex = hoveredSnippet;
                        hoveredMessageIndex = messageIndex;
                        snippetIndex = lineOffset;
                    }

                    displayedLines++;
                    lineOffset++;

                    int msgLineCount = (int)msg.GetType().GetProperty("LineCount", BindingFlags.Public | BindingFlags.Instance)!.GetValue(msg)!;
                    if (lineOffset >= msgLineCount)
                    {
                        lineOffset = 0;
                        messageIndex++;
                    }
                }

                // Hover/click handling (unchanged)
                if (hoveredMessageIndex.HasValue && hoveredSnippetIndex.HasValue)
                {
                    var msg = messages[hoveredMessageIndex.Value];
                    var getSnip = msg.GetType().GetMethod("GetSnippetWithInversedIndex", BindingFlags.Public | BindingFlags.Instance)!;
                    var snips = (TextSnippet[])getSnip.Invoke(msg, new object[] { snippetIndex })!;
                    var snip = snips[hoveredSnippetIndex.Value];
                    snip.OnHover();
                    if (Main.mouseLeft && Main.mouseLeftRelease) snip.OnClick();
                }
            })
        );
    }

    /// <summary>
    /// The action to perform when the hook is hooking
    /// </summary>
    private void ModifyLinkSnippetHook(object self, string text, bool force, Color c, int widthLimit)
    {
        var parsedList = GetParsedList(self);

        var line = parsedList[^1];

        for (int i = 0; i < line.Length; i++)
        {
            var snip = line[i];
            if (snip == null) continue;
            var t = snip.Text?.Trim();
            if (string.IsNullOrEmpty(t)) continue;

            if (LinkTagHandler.ContainsLink(t) && snip is not MentionSnippet)
            {
                line[i] = new LinkSnippet(snip)
                {
                    CheckForHover = true
                };
            }
        }
    }

    /// <summary>
    /// Adds 8 empty text snippets to a message if it contains an upload.
    /// </summary>
    // Adds 8 padding lines (counted for spacing) that we’ll skip drawing in RenderChat
    private void ModifyUploadSnippetHook(object self, string text, bool force, Color c, int widthLimit)
    {
        if (!UploadTagHandler.ContainsUploadTag(text)) return;

        var parsedList = GetParsedList(self);
        for (int i = 0; i < 8; i++)
            parsedList.Add([new PadSnippet()]);
    }

    private void InitializeNewTextHook(Mod cc)
    {
        var newTextMethod = GetMethodNewTextMultilineMethod(cc);
        if (newTextMethod != null)
        {
            newTextHook = new Hook(
                newTextMethod,
                new Action<newTextOrig, object, string, bool, Color, int>((orig, self, text, force, c, widthLimit) =>
                {
                    orig(self, text, force, c, widthLimit); // call vanilla behaviour
                    ModifyLinkSnippetHook(self, text, force, c, widthLimit); // call our action
                    ModifyUploadSnippetHook(self, text, force, c, widthLimit);
                }
            ));
        }
    }

    private List<TextSnippet[]> GetParsedList(object self)
    {
        // Get vanillaChatRoom
        var vanillaField = self.GetType().GetField("vanillaChatRoom", BindingFlags.NonPublic | BindingFlags.Instance);
        var vanilla = vanillaField.GetValue(self);

        // Get messages
        var msgsField = vanilla.GetType().GetField("messages", BindingFlags.NonPublic | BindingFlags.Instance);
        var msgs = msgsField.GetValue(vanilla) as System.Collections.IList;

        var container = msgs[0]; // newest message is inserted at index 0

        // Get parsedText
        var parsedField = container.GetType().GetField("parsedText", BindingFlags.NonPublic | BindingFlags.Instance);
        var parsedList = parsedField.GetValue(container) as List<TextSnippet[]>;

        return parsedList;
    }

    private MethodInfo GetMethodNewTextMultilineMethod(Mod cc)
    {
        var type = cc.Code.GetType("Tomat.TML.Mod.ChitterChatter.Content.Features.ChatMonitor.CustomChatMonitor");
        if (type == null)
        {
            Log.Error($"CC not found, exiting...");
            return null;
        }
        string iface = typeof(IChatMonitor).FullName;// "Terraria.GameContent.UI.Chat.IChatMonitor"

        var method = type.GetMethod(
            iface + ".NewTextMultiline",
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(string), typeof(bool), typeof(Color), typeof(int)],
            null
        );

        if (method == null)
        {
            Log.Error($"NewText not found, exiting...");
            return null;
        }
        return method;
    }

    private sealed class PadSnippet : TextSnippet
    {
        public PadSnippet() : base(" ") { }
    }
}
