using System.Reflection;
using ChatPlus.Core.Features.ModIcons;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ChatPlus.Common.ModCommands;
public class ChatClearCommand : ModCommand
{
    public static LocalizedText UsageText { get; private set; }
    public static LocalizedText DescriptionText { get; private set; }
    public static LocalizedText ClearedText { get; private set; }

    public override void SetStaticDefaults()
    {
        string key = $"Commands.{nameof(ChatClearCommand)}.";

        UsageText = Mod.GetLocalization($"{key}Usage");
        DescriptionText = Mod.GetLocalization($"{key}Description");
        ClearedText = Mod.GetLocalization($"{key}Cleared");
    }

    public override CommandType Type => CommandType.Chat;
    public override string Command => "clear";
    public override string Usage => UsageText.Value;
    public override string Description => DescriptionText.Value;

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        ClearChatMonitor();
    }

    private void ClearChatMonitor()
    {
        // Clear Main.chatMonitor messages
        var clearMethod = Main.chatMonitor.GetType()
            .GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public);
        clearMethod?.Invoke(Main.chatMonitor, null);

        // Show localized confirmation message
        if (ModLoader.TryGetMod("ChatPlus", out Mod chatPlus))
        {
            string tag = ModIconTagHandler.GenerateTag(chatPlus.Name);
            tag = null;
            //Main.NewTextMultiline(tag + ClearedText.Value, c: Color.Green);
            //Main.NewText(tag + ClearedText.Value, Color.Green);
        }
    }
}
