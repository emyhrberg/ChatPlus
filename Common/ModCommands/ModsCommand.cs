using System;
using System.Linq;
using System.Reflection;
using ChatPlus.Core.Features.ModIcons;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ChatPlus.Common.ModCommands;
public class ModsCommand : ModCommand
{
    public static LocalizedText UsageText { get; private set; }
    public static LocalizedText DescriptionText { get; private set; }
    public override void SetStaticDefaults()
    {
        string key = $"Commands.{nameof(ModsCommand)}.";

        UsageText = Mod.GetLocalization($"{key}Usage");
        DescriptionText = Mod.GetLocalization($"{key}Description");
    }

    public override CommandType Type => CommandType.Chat;
    public override string Command => "mods";
    public override string Usage => UsageText.Value;
    public override string Description => DescriptionText.Value;

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        PrintMods();
    }

    private void PrintMods()
    {
        Main.NewText(" [c/fff014:Mods:]");

        // Collect mods and sort alphabetically by display name
        var mods = ModLoader.Mods
            .Where(m => m != null)
            .OrderBy(m => string.IsNullOrEmpty(m.DisplayName) ? m.Name : m.DisplayName, StringComparer.OrdinalIgnoreCase);

        foreach (var mod in mods)
        {
            string internalName = mod.Name;
            string displayName = string.IsNullOrEmpty(mod.DisplayName) ? internalName : mod.DisplayName;

            // Try to register and generate icon tag
            string tag = ModIconTagHandler.GenerateTag(internalName);

            // Print line with icon and display name
            Main.NewText($" {tag} {displayName}", Color.White);
        }
    }
}
