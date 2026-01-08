using ChatPlus.Common.Compat.CustomTags;
using ChatPlus.Common.Debug;
using ChatPlus.Core.Features.Colors;
using ChatPlus.Core.Features.Commands;
using ChatPlus.Core.Features.Emojis;
using ChatPlus.Core.Features.Glyphs;
using ChatPlus.Core.Features.Items;
using ChatPlus.Core.Features.Mentions;
using ChatPlus.Core.Features.ModIcons;
using ChatPlus.Core.Features.PlayerColors;
using ChatPlus.Core.Features.PlayerIcons;
using ChatPlus.Core.Features.Stats.PlayerStats.StatsPrivacy;
using ChatPlus.Core.Features.TypingIndicators;
using ChatPlus.Core.Features.Uploads;
using ChatPlus.Core.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChatPlus;

public class ChatPlus : Mod
{
    public static StateManager StateManager { get; private set; }

    public override void Load()
    {
        StateManager = new StateManager(
            ModContent.GetInstance<CommandSystem>(),
            ModContent.GetInstance<ColorSystem>(),
            ModContent.GetInstance<EmojiSystem>(),
            ModContent.GetInstance<GlyphSystem>(),
            ModContent.GetInstance<ItemSystem>(),
            ModContent.GetInstance<ModIconSystem>(),
            ModContent.GetInstance<MentionSystem>(),
            ModContent.GetInstance<PlayerIconSystem>(),
            ModContent.GetInstance<UploadSystem>(),
            ModContent.GetInstance<CustomTagSystem>()
        );
    }

    public override void Unload()
    {
        StateManager = null;
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        PacketType type = (PacketType)reader.ReadByte();

        switch (type)
        {
            case PacketType.TypingIndicator:
                TypingIndicatorNetHandler.Receive(reader, whoAmI);
                break;

            case PacketType.Upload:
                UploadNetHandler.Receive(reader, whoAmI);
                break;

            case PacketType.PlayerColor:
                PlayerColorNetHandler.Receive(reader, whoAmI);
                break;

            case PacketType.Privacy:
                PrivacyNetHandler.Receive(reader, whoAmI);
                break;

            default:
                Log.Error($"Unknown packet type: {type}");
                break;
        }
    }

    /// <summary>
    /// A custom call method for inter-mod communication.
    /// </summary>
    public override object Call(params object[] args)
    {
        if (args.Length == 0 || args[0] is not string command)
            throw new ArgumentException("ChatPlus Mod Call: First argument must be a command string.");

        if (string.Equals(command, "RegisterTag"))
        {
            if (args.Length != 3)
                throw new ArgumentException("ChatPlus Mod Call: RegisterTag expects 3 args.");

            if (args[1] is not string tag)
                throw new ArgumentException("ChatPlus Mod Call: Second arg must be tag string.");

            if (args[2] is not IEnumerable<(string actualTag, UIElement view)> tagsToAdd)
                throw new ArgumentException("ChatPlus Mod Call: Third arg must be IEnumerable<(string, UIElement)>.");

            foreach (var (actualTag, view) in tagsToAdd)
            {
                var ct = new CustomTag(tag, actualTag, view);
                CustomTagSystem.CustomTags.Add(ct);
            }

            if (!CustomTagSystem.States.ContainsKey(tag))
                CustomTagSystem.States[tag] = new CustomTagState(tag);

            return null;
        }

        if (string.Equals(command, "RegisterTagProvider"))
        {
            if (args.Length != 3)
                throw new ArgumentException("ChatPlus Mod Call: RegisterTagProvider expects 3 args.");

            if (args[1] is not string tag)
                throw new ArgumentException("ChatPlus Mod Call: Second arg must be tag string.");

            if (args[2] is not Func<string, IEnumerable<(string, UIElement)>> provider)
                throw new ArgumentException("ChatPlus Mod Call: Third arg must be Func<string, IEnumerable<(string, UIElement)>>.");

            CustomTagSystem.Providers[tag] = provider;

            if (!CustomTagSystem.States.ContainsKey(tag))
                CustomTagSystem.States[tag] = new CustomTagState(tag);

            return null;
        }

        throw new ArgumentException($"ChatPlus Mod Call: Unknown command '{command}'.");
    }
}