using System;
using System.ComponentModel;
using System.Reflection;
using ChatPlus.Common.Configs.ConfigElements;
using ChatPlus.Common.Configs.ConfigElements.ButtonConfigElements;
using ChatPlus.Common.Debug;
using ChatPlus.Core.Chat.ChatButtons;
using ChatPlus.Core.Chat.ChatButtons.Shared;
using ChatPlus.Core.Features.Mentions;
using ChatPlus.Core.Features.PlayerColors;
using ChatPlus.Core.Features.Stats.PlayerStats.Privacy;
using ChatPlus.Core.Features.Stats.PlayerStats.StatsPrivacy;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Terraria;
using Terraria.GameContent.UI.Chat;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace ChatPlus.Common.Configs;

/// <summary>
/// Client-sided config.
/// </summary>
public class Config : ModConfig
{
    #region Enums
    public enum Privacy
    {
        NoOne,
        Team,
        Everyone
    }
    public enum TimestampSettings
    {
        Off,
        HourAndMinute12Hours,
        HourAndMinuteAndSeconds12Hours,
        HourAndMinute24Hours,
        HourAndMinuteAndSeconds24Hours,
    }
    public enum Viewmode
    {
        List,
        Grid
    }

    #endregion
    #region Fields
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("ChatSettings")]

    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool Autocomplete = true;

    [BackgroundColor(255, 192, 8)]
    [Range(10f, 20f)]
    [Increment(1f)]
    [DefaultValue(10f)]
    [DrawTicks]
    public float AutocompleteItemsVisible = 10f;

    [BackgroundColor(255, 192, 8)] // Golden Yellow
    [DefaultValue(true)]
    public bool Scrollbar = true;

    [BackgroundColor(255, 192, 8)] // Golden Yellow
    [DefaultValue(true)]
    public bool TextEditor = true;

    [BackgroundColor(255, 192, 8)] // Golden Yellow
    [Range(10f, 20f)]
    [Increment(1f)]
    [DefaultValue(10f)]
    [DrawTicks]
    public float ChatsVisible = 10f;

    // -----------------------------------------------------------

    [Header("ChatFormat")]

    [CustomModConfigItem(typeof(TimestampConfigElement<TimestampSettings>))]
    [BackgroundColor(128, 255, 128)] // Grass Green
    [DefaultValue(TimestampSettings.Off)]
    [JsonConverter(typeof(StringEnumConverter))]
    public TimestampSettings timestampSettings;

    [CustomModConfigItem(typeof(TypingIndicatorsConfigElement))]
    [BackgroundColor(128, 255, 128)] // Grass Green
    [DefaultValue(true)]
    public bool TypingIndicators = true;

    [CustomModConfigItem(typeof(ModIconsConfigElement))]
    [BackgroundColor(128, 255, 128)] // Grass Green
    [DefaultValue(true)]
    public bool ModIcon;

    [CustomModConfigItem(typeof(PlayerIconsConfigElement))]
    [BackgroundColor(128, 255, 128)] // Grass Green
    [DefaultValue(true)]
    public bool PlayerIcon;

    [BackgroundColor(128, 255, 128)] // Grass Green
    [CustomModConfigItem(typeof(PlayerColorConfigElement))]
    [DefaultValue("FFFFFF")]
    public string PlayerColor;

    // -----------------------------------------------------------

    [Header("StatsViewer")]

    [BackgroundColor(192, 54, 64)] // Calamity Red
    [DefaultValue(true)]
    public bool ShowStatsWhenHovering;

    [BackgroundColor(192, 54, 64)] // Calamity Red
    [DefaultValue(true)]
    public bool OpenImageWhenClicking;

    [BackgroundColor(192, 54, 64)] // Calamity Red
    [DefaultValue(false)]
    public bool ShowStatsWhenBossIsAlive;

    [BackgroundColor(192, 54, 64)] // Calamity Red
    [DefaultValue(Privacy.Everyone)]
    [JsonConverter(typeof(StringEnumConverter))]
    public Privacy StatsPrivacy;

    // -----------------------------------------------------------

    [Header("ShowButtons")]

    [CustomModConfigItem(typeof(ConfigButtonConfigElement))]
    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool ShowConfigButton;

    [CustomModConfigItem(typeof(ViewmodeButtonConfigElement))]
    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool ShowViewmodeButton;

    [CustomModConfigItem(typeof(ColorButtonConfigElement))]
    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool ShowColorButton;

    [CustomModConfigItem(typeof(CommandButtonConfigElement))]
    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool ShowCommandButton;

    [CustomModConfigItem(typeof(EmojiButtonConfigElement))]
    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool ShowEmojiButton;

    [CustomModConfigItem(typeof(GlyphButtonConfigElement))]
    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool ShowGlyphButton;

    [CustomModConfigItem(typeof(ItemButtonConfigElement))]
    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool ShowItemButton;

    [CustomModConfigItem(typeof(MentionButtonConfigElement))]
    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool ShowMentionButton;

    [CustomModConfigItem(typeof(ModIconButtonConfigElement))]
    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool ShowModIconButton;

    [CustomModConfigItem(typeof(PlayerIconButtonConfigElement))]
    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool ShowPlayerIconButton;

    [CustomModConfigItem(typeof(UploadButtonConfigElement))]
    [BackgroundColor(255, 192, 8)]
    [DefaultValue(true)]
    public bool ShowUploadButton;

    [Header("Preview")]

    [CustomModConfigItem(typeof(ChatBoxPreviewElement))]
    public int ChatBoxPreview;
    #endregion Fields
    #region OnChanged
    public override void OnChanged()
    {
        base.OnChanged();

        if (Conf.C == null) return;

        UpdateShowCount();
        UpdateChatButtons();
        UpdatePlayerColor();
        UpdateStatsPrivacy();
    }

    private void UpdateShowCount()
    {
        var field = typeof(RemadeChatMonitor)
        .GetField("_showCount", BindingFlags.Instance | BindingFlags.NonPublic);

        if (field != null)
        {
            field.SetValue(Main.chatMonitor, (int)Conf.C.ChatsVisible);
        }
    }

    private void UpdateStatsPrivacy()
    {
        PrivacyCache.Set(Main.myPlayer, StatsPrivacy);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            PrivacyNetHandler.SendLocalPrivacy();
        }
        else if (Main.netMode == NetmodeID.Server)
        {
            PrivacyNetHandler.BroadcastSingle(Main.myPlayer, StatsPrivacy);
        }
    }

    private void UpdateChatButtons()
    {
        if (!Main.gameMenu)
        {
            var sys = ModContent.GetInstance<ChatButtonsSystem>();
            if (sys == null) return;

            var state = new UIState();
            var cfg = Conf.C;

            if (cfg?.ShowEmojiButton ?? true) state.Append(new EmojiButton());
            if (cfg?.ShowUploadButton ?? true) state.Append(new UploadButton());
            if (cfg?.ShowColorButton ?? true) state.Append(new ColorButton());
            if (cfg?.ShowCommandButton ?? true) state.Append(new CommandButton());
            if (cfg?.ShowGlyphButton ?? true) state.Append(new GlyphButton());
            if (cfg?.ShowItemButton ?? true) state.Append(new ItemButton());
            if (cfg?.ShowMentionButton ?? true) state.Append(new MentionButton());
            if (cfg?.ShowConfigButton ?? true) state.Append(new ConfigButton());
            if (cfg?.ShowModIconButton ?? true) state.Append(new ModIconButton());
            if (cfg?.ShowPlayerIconButton ?? true) state.Append(new PlayerIconButton());

            sys.ui?.SetState(state);
            sys.state = state; // update reference
        }
    }

    private void UpdatePlayerColor()
    {
        try
        {
            string hex = (Conf.C.PlayerColor ?? "FFFFFF")
                .Trim()
                .TrimStart('#')
                .ToUpperInvariant();

            if (Main.LocalPlayer != null)
            {
                PlayerColorSystem.PlayerColors[Main.myPlayer] = hex;
                MentionSnippet.InvalidateCachesFor(Main.LocalPlayer.name);
                Log.Info("Player color updated to: " + hex + " for: " + Main.LocalPlayer.name);
            }

            if (Main.netMode == NetmodeID.MultiplayerClient && Main.LocalPlayer != null)
            {
                PlayerColorNetHandler.ClientHello(Main.myPlayer, hex);
            }
            MentionSnippet.ClearAllCaches();
        }
        catch (Exception e)
        {
            Log.Error("UpdatePlayerColor Exception: " + e);
        }
    }
}
#endregion
public static class Conf
{
    public static Config C => ModContent.GetInstance<Config>();
}