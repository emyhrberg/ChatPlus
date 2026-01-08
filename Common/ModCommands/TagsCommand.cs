using System.Linq;
using ChatPlus.Core.Features.ModIcons;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ChatPlus.Common.ModCommands;
public class TagsCommand : ModCommand
{
    public static LocalizedText UsageText { get; private set; }
    public static LocalizedText DescriptionText { get; private set; }
    public static LocalizedText HeaderText { get; private set; }
    public static LocalizedText[] TagTexts { get; private set; }

    public override void SetStaticDefaults()
    {
        string key = $"Commands.{nameof(TagsCommand)}.";

        UsageText = Mod.GetLocalization($"{key}Usage");
        DescriptionText = Mod.GetLocalization($"{key}Description");
        HeaderText = Mod.GetLocalization($"{key}TagsHeader");

        TagTexts = Enumerable.Range(1, 7).Select(i => Mod.GetLocalization($"{key}Tag_{i}")).ToArray();
    }

    public override CommandType Type => CommandType.Chat;
    public override string Command => "tags";
    public override string Usage => UsageText.Value;

    public override string Description => DescriptionText.Value;

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        string key = $"Commands.{nameof(TagsCommand)}.";
        TagTexts = Enumerable.Range(1, 7).Select(i => Mod.GetLocalization($"{key}Tag_{i}")).ToArray();

        if (ModLoader.TryGetMod("ChatPlus", out Mod chatPlus))
        {
            string tag = ModIconTagHandler.GenerateTag(chatPlus.Name);
            tag = null;

            // Header
            Main.NewText(HeaderText.Format(tag + " " + "[c/fff014:", "]"));

            // Each tag line
            foreach (var tagText in TagTexts)
            {
                Main.NewText(tag + " " + tagText.Value, Color.White);
            }
        }
    }
}
