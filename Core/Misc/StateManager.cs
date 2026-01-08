using System;
using ChatPlus.Common.Compat.CustomTags;
using ChatPlus.Common.Configs;
using ChatPlus.Core.Chat;
using ChatPlus.Core.Features.Colors;
using ChatPlus.Core.Features.Commands;
using ChatPlus.Core.Features.Emojis;
using ChatPlus.Core.Features.Glyphs;
using ChatPlus.Core.Features.Items;
using ChatPlus.Core.Features.Mentions;
using ChatPlus.Core.Features.ModIcons;
using ChatPlus.Core.Features.PlayerIcons;
using ChatPlus.Core.Features.Uploads;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;

namespace ChatPlus.Core.Misc;

/// <summary>
/// Provides centralized management and coordination of UI state systems for chat-related features.
/// </summary>
public class StateManager
{
    public CommandSystem CommandSystem { get; }
    public ColorSystem ColorSystem { get; }
    public EmojiSystem EmojiSystem { get; }
    public GlyphSystem GlyphSystem { get; }
    public ItemSystem ItemSystem { get; }
    public ModIconSystem ModIconSystem { get; }
    public MentionSystem MentionSystem { get; }
    public PlayerIconSystem PlayerIconSystem { get; }
    public UploadSystem UploadSystem { get; }
    public CustomTagSystem CustomTagSystem { get; }

    public StateManager(
        CommandSystem commandSystem,
        ColorSystem colorSystem,
        EmojiSystem emojiSystem,
        GlyphSystem glyphSystem,
        ItemSystem itemSystem,
        ModIconSystem modIconSystem,
        MentionSystem mentionSystem,
        PlayerIconSystem playerIconSystem,
        UploadSystem uploadSystem,
        CustomTagSystem customTagSystem)
    {
        CommandSystem = commandSystem;
        ColorSystem = colorSystem;
        EmojiSystem = emojiSystem;
        GlyphSystem = glyphSystem;
        ItemSystem = itemSystem;
        ModIconSystem = modIconSystem;
        MentionSystem = mentionSystem;
        PlayerIconSystem = playerIconSystem;
        UploadSystem = uploadSystem;
        CustomTagSystem = customTagSystem;
    }

    public void OpenStateByTriggers(GameTime gameTime, UserInterface ui, UIState state, params ITrigger[] triggers)
    {
        if (!Main.drawingPlayerChat)
        {
            CloseAll();
            ResetAllWasOpenedByButton();
            return;
        }

        if (!Conf.C.Autocomplete)
        {
            if (IsOpenedByButton(state))
            {
                if (ui.CurrentState != state)
                {
                    OpenExclusive(ui, state);
                }
                ui.Update(gameTime);
            }
            else
            {
                if (ui.CurrentState == state)
                {
                    ui.SetState(null);
                }
            }
            return;
        }

        string text = Main.chatText ?? string.Empty;
        int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);

        bool shouldOpen = false;
        if (triggers != null && triggers.Length > 0)
        {
            for (int i = 0; i < triggers.Length; i++)
            {
                if (triggers[i] != null && triggers[i].ShouldOpen(text, caret))
                {
                    shouldOpen = true;
                    break;
                }
            }
        }

        if (shouldOpen)
        {
            // Prefix wins: replace whatever is open with this state
            ResetAllWasOpenedByButton();
            if (ui.CurrentState != state)
            {
                OpenExclusive(ui, state);
            }
            ui.Update(gameTime);
            return;
        }

        // No active prefix: only keep this panel if it was opened by its button
        if (IsOpenedByButton(state))
        {
            if (ui.CurrentState != state)
            {
                OpenExclusive(ui, state);
            }
            ui.Update(gameTime);
            return;
        }

        if (ui.CurrentState == state)
        {
            ui.SetState(null);
        }
    }

    public void OpenExclusive(UserInterface ui, UIState state)
    {
        CloseAll();
        if (ui != null && state != null)
        {
            ui.SetState(state);
        }
    }

    private void CloseAll()
    {
        void Close(UserInterface u)
        {
            if (u != null && u.CurrentState != null)
            {
                u.SetState(null);
            }
        }

        Close(CommandSystem?.ui);
        Close(ColorSystem?.ui);
        Close(EmojiSystem?.ui);
        Close(GlyphSystem?.ui);
        Close(ItemSystem?.ui);
        Close(ModIconSystem?.ui);
        Close(PlayerIconSystem?.ui);
        Close(UploadSystem?.ui);
        Close(MentionSystem?.ui);
        Close(CustomTagSystem?.ui);
    }

    private static bool IsOpenedByButton(UIState state)
    {
        if (state is EmojiState) return EmojiState.WasOpenedByButton;
        if (state is UploadState) return UploadState.WasOpenedByButton;
        if (state is ColorState) return ColorState.WasOpenedByButton;
        if (state is ItemState) return ItemState.WasOpenedByButton;
        if (state is CommandState) return CommandState.WasOpenedByButton;
        if (state is GlyphState) return GlyphState.WasOpenedByButton;
        if (state is MentionState) return MentionState.WasOpenedByButton;
        if (state is ModIconState) return ModIconState.WasOpenedByButton;
        if (state is PlayerIconState) return PlayerIconState.WasOpenedByButton;
        if (state is CustomTagState) return CustomTagState.WasOpenedByButton;
        return false;
    }

    public DraggablePanel GetActivePanel()
    {
        DraggablePanel Try(UIState st)
        {
            if (st == null) return null;
            var prop = st.GetType().GetProperty("Panel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            return prop?.GetValue(st) as DraggablePanel;
        }

        // return the panel for whichever UI is currently open
        if (CommandSystem?.ui?.CurrentState == CommandSystem?.state) return Try(CommandSystem.state);
        if (ColorSystem?.ui?.CurrentState == ColorSystem?.state) return Try(ColorSystem.state);
        if (EmojiSystem?.ui?.CurrentState == EmojiSystem?.state) return Try(EmojiSystem.state);
        if (GlyphSystem?.ui?.CurrentState == GlyphSystem?.state) return Try(GlyphSystem.state);
        if (ItemSystem?.ui?.CurrentState == ItemSystem?.state) return Try(ItemSystem.state);
        if (ModIconSystem?.ui?.CurrentState == ModIconSystem?.state) return Try(ModIconSystem.state);
        if (PlayerIconSystem?.ui?.CurrentState == PlayerIconSystem?.state) return Try(PlayerIconSystem.state);
        if (UploadSystem?.ui?.CurrentState == UploadSystem?.state) return Try(UploadSystem.state);
        if (MentionSystem?.ui?.CurrentState == MentionSystem?.state) return Try(MentionSystem.state);

        return null;
    }

    public bool IsAnyStateActive()
    {
        return CommandSystem?.ui?.CurrentState != null ||
               ColorSystem?.ui?.CurrentState != null ||
               EmojiSystem?.ui?.CurrentState != null ||
               GlyphSystem?.ui?.CurrentState != null ||
               ItemSystem?.ui?.CurrentState != null ||
               ModIconSystem?.ui?.CurrentState != null ||
               PlayerIconSystem?.ui?.CurrentState != null ||
               UploadSystem?.ui?.CurrentState != null ||
               MentionSystem?.ui?.CurrentState != null ||
               CustomTagSystem?.ui?.CurrentState != null;
    }

    public void CloseOthers(UserInterface keep)
    {
        void Close(UserInterface u)
        {
            if (u != null && u != keep && u.CurrentState != null)
                u.SetState(null);
        }

        Close(CommandSystem?.ui);
        Close(ColorSystem?.ui);
        Close(EmojiSystem?.ui);
        Close(GlyphSystem?.ui);
        Close(ItemSystem?.ui);
        Close(ModIconSystem?.ui);
        Close(PlayerIconSystem?.ui);
        Close(UploadSystem?.ui);
        Close(MentionSystem?.ui);
        Close(CustomTagSystem?.ui);
    }
    public static bool IsAnyGridPanelActive()
    {
        var sm = ChatPlus.StateManager;
        if (sm == null) return false;

        return
            sm.CommandSystem?.state?.Panel?.IsGridModeEnabled == true ||
            sm.ColorSystem?.state?.Panel?.IsGridModeEnabled == true ||
            sm.EmojiSystem?.state?.Panel?.IsGridModeEnabled == true ||
            sm.GlyphSystem?.state?.Panel?.IsGridModeEnabled == true ||
            sm.ItemSystem?.state?.Panel?.IsGridModeEnabled == true ||
            sm.ModIconSystem?.state?.Panel?.IsGridModeEnabled == true ||
            sm.PlayerIconSystem?.state?.Panel?.IsGridModeEnabled == true ||
            sm.UploadSystem?.state?.Panel?.IsGridModeEnabled == true ||
            sm.MentionSystem?.state?.Panel?.IsGridModeEnabled == true;
    }

    public void ResetAllWasOpenedByButton()
    {
        CommandState.WasOpenedByButton = false;
        ColorState.WasOpenedByButton = false;
        EmojiState.WasOpenedByButton = false;
        GlyphState.WasOpenedByButton = false;
        ItemState.WasOpenedByButton = false;
        ModIconState.WasOpenedByButton = false;
        MentionState.WasOpenedByButton = false;
        PlayerIconState.WasOpenedByButton = false;
        UploadState.WasOpenedByButton = false;
        CustomTagState.WasOpenedByButton = false;
    }
}
