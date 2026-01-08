using System;
using System.Collections.Generic;
using ChatPlus.Common.Debug;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Commands;
internal class CommandManager : ModSystem
{
    public static List<Command> Commands { get; private set; } = [];

    // Do NOT use Load to initialize commands.
    // Use a hook where other mods have had a chance to load their commands
    public override void PostSetupContent()
    {
        Commands.Clear();
        InitializeCommands();
    }

    private void InitializeCommands()
    {
        int index = 1;

        // Load commands from ModCommand
        foreach (var cmdList in CommandLoader.Commands)
        {
            foreach (ModCommand cmd in cmdList.Value)
            {
                if (cmd.Command == "help")
                    continue;

                string name = "/" + cmd.Command;
                string usage = cmd.Description;
                Mod mod = cmd.Mod;

                Commands.Add(new Command(name, usage, mod));
                Log.Info($"({index++}) Command: \"{name}\" loaded from \"{mod?.Name}\"");
            }
        }

        // Load vanilla commands from ChatManager
        foreach (var cmdId in ChatManager.Commands._localizedCommands)
        {
            string name = cmdId.Key.Value;
            string localizationKey = cmdId.Value._name;
            string usage = Language.GetTextValue("ChatCommandDescription." + localizationKey);

            Commands.Add(new Command(name, usage));
            Log.Info($"({index++}) Command: \"{name}\" loaded from \"Terraria\"");
        }

        // Sort by mod name, then command name
        Commands.Sort((a, b) =>
        {
            // Set Terraria mod name
            string am = a.Mod != null
                ? a.Mod.DisplayNameClean ?? a.Mod.Name
                : "Terraria";
            string bm = b.Mod != null
                ? b.Mod.DisplayNameClean ?? b.Mod.Name
                : "Terraria";

            int modCmp = string.Compare(am, bm, StringComparison.OrdinalIgnoreCase);
            if (modCmp != 0) return modCmp;

            string an = a.Name.StartsWith('/') ? a.Name.Substring(1) : a.Name;
            string bn = b.Name.StartsWith('/') ? b.Name.Substring(1) : b.Name;

            return string.Compare(an, bn, StringComparison.OrdinalIgnoreCase);
        });

        Log.Info($"Total commands loaded: {Commands.Count}");
    }
}
