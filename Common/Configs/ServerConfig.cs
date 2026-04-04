using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using static ChatPlus.Common.Configs.ClientConfig;

namespace ChatPlus.Common.Configs;

/// <summary>
/// Server-sided config.
/// </summary>
public class ServerConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    public enum TypingIndicatorsOverrideMode
    {
        LetClientsDecide,
        NoOne,
        Team,
        Everyone
    }

    [Header("TypingIndicators")]
    [BackgroundColor(128, 255, 128)]
    [JsonConverter(typeof(StringEnumConverter))]
    [DefaultValue(TypingIndicatorsOverrideMode.LetClientsDecide)]
    public TypingIndicatorsOverrideMode OverrideTypingIndicators = TypingIndicatorsOverrideMode.LetClientsDecide;

    [Header("Uploads")]
    [BackgroundColor(120, 90, 180, 220)]
    [DefaultValue(true)]
    public bool AllowImageUploads = true;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            message = NetworkText.FromLiteral("Saved!");
            return true;
        }

        if (whoAmI < 0 || whoAmI >= Main.maxPlayers || !Main.player[whoAmI].active)
        {
            message = NetworkText.FromLiteral("Invalid client.");
            return false;
        }

        Player player = Main.player[whoAmI];

        if (TryDragonLensAdmin(player, out bool dragonLensAdmin))
        {
            if (dragonLensAdmin)
            {
                message = NetworkText.FromLiteral("Saved!");
                return true;
            }

            message = NetworkText.FromLiteral("You must be a DragonLens admin to modify this config.");
            return false;
        }

        message = NetworkText.FromLiteral("You are not allowed to modify this server config.");
        return false;
    }

    private static bool TryDragonLensAdmin(Player player, out bool isAdmin)
    {
        isAdmin = false;

        if (!ModLoader.TryGetMod("DragonLens", out Mod dragonLens))
            return false;

        Type type = dragonLens.Code?.GetType("DragonLens.Core.Systems.PermissionHandler");
        MethodInfo method = type?.GetMethod("CanUseTools", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (method is null)
            return false;

        isAdmin = method.Invoke(null, [player]) is bool b && b;
        return true;
    }
}