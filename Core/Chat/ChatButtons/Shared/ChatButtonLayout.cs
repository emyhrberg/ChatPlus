using System;
using System.Collections.Generic;
using ChatPlus.Common.Configs;
using ChatPlus.Core.Features.Colors;
using ChatPlus.Core.Features.Commands;
using ChatPlus.Core.Features.Emojis;
using ChatPlus.Core.Features.Glyphs;
using ChatPlus.Core.Features.Items;
using ChatPlus.Core.Features.Mentions;
using ChatPlus.Core.Features.ModIcons;
using ChatPlus.Core.Features.PlayerIcons;
using ChatPlus.Core.Features.Uploads;

using Terraria;
using static ChatPlus.Common.Configs.ClientConfig;

namespace ChatPlus.Core.Chat.ChatButtons.Shared;

public enum ChatButtonType
{
    Commands,
    Colors,
    Emojis,
    Glyphs,
    Items,
    ModIcons,
    Mentions,
    PlayerIcons,
    Uploads,
    Config,
    Viewmode
}

public static class ChatButtonLayout
{
    public static readonly ChatButtonType[] OrderRightToLeft =
    [
        ChatButtonType.Uploads,
        ChatButtonType.PlayerIcons,
        ChatButtonType.ModIcons,
        ChatButtonType.Mentions,
        ChatButtonType.Items,
        ChatButtonType.Glyphs,
        ChatButtonType.Emojis,
        ChatButtonType.Colors,
        ChatButtonType.Commands,

        // extra
        ChatButtonType.Viewmode,
        ChatButtonType.Config,
    ];
    public static readonly Dictionary<Type, Viewmode> DefaultViewmodes = new()
    {
        { typeof(CommandPanel),    Viewmode.List },
        { typeof(ColorPanel),      Viewmode.Grid },
        { typeof(EmojiPanel),      Viewmode.Grid },
        { typeof(GlyphPanel),      Viewmode.Grid },
        { typeof(ItemPanel),       Viewmode.Grid },
        { typeof(MentionPanel),    Viewmode.List },
        { typeof(ModIconPanel),    Viewmode.List },
        { typeof(PlayerIconPanel), Viewmode.List },
        { typeof(UploadPanel),     Viewmode.Grid },
    };
    public static Viewmode GetViewmodeFor(Type panelType)
    {
        return DefaultViewmodes.TryGetValue(panelType, out var vm) ? vm : Viewmode.List;
    }
    public static bool IsEnabled(ChatButtonType type)
    {
        var s = Conf.C;
        if (s == null) return true; // default to visible when config not initialized

        return type switch
        {
            ChatButtonType.Commands => s.ShowCommandButton,
            ChatButtonType.Colors => s.ShowColorButton,
            ChatButtonType.Emojis => s.ShowEmojiButton,
            ChatButtonType.Glyphs => s.ShowGlyphButton,
            ChatButtonType.Items => s.ShowItemButton,
            ChatButtonType.ModIcons => s.ShowModIconButton,
            ChatButtonType.Mentions => s.ShowMentionButton,
            ChatButtonType.PlayerIcons => s.ShowPlayerIconButton,
            ChatButtonType.Uploads => s.ShowUploadButton,
            ChatButtonType.Config => s.ShowConfigButton,
            ChatButtonType.Viewmode => s.ShowViewmodeButton,
            _ => true
        };
    }

    public static int VisibleIndex(ChatButtonType type)
    {
        var enabled = new List<ChatButtonType>(OrderRightToLeft.Length);
        foreach (var button in OrderRightToLeft)
        {
            if (IsEnabled(button))
            {
                //Log.Debug(type);
                enabled.Add(button);
            }
        }

        return enabled.IndexOf(type); // 0 = rightmost
    }

    public static Vector2 ComputeTopLeft(ChatButtonType type, int size = 24, int gap = 3)
    {
        int index = VisibleIndex(type);
        //Log.Info($"{type} {index}");
        if (index < 0)
        {
            index = 0;
        }

        float baseX = Main.screenWidth - 255;
        float x = baseX - index * (size + gap + 1);
        float y = Main.screenHeight - 59;
        return new Vector2(x, y);
    }
}
