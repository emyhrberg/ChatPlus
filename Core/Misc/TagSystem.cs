using ChatPlus.Core.Features.BoldText;
using ChatPlus.Core.Features.Emojis;
using ChatPlus.Core.Features.Links;
using ChatPlus.Core.Features.Mentions;
using ChatPlus.Core.Features.ModIcons;
using ChatPlus.Core.Features.PlayerIcons;
using ChatPlus.Core.Features.UnderlineText;
using ChatPlus.Core.Features.Uploads;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Misc;

internal class TagSystem : ModSystem
{
    /// <summary>
    /// Registers all tag handlers with their respective tags.
    /// Avoid colliding with vanilla tags:
    // ChatManager.Register<ColorTagHandler>(new string[] { "c", "color" });
    // ChatManager.Register<ItemTagHandler>(new string[] { "i", "item" });
    // ChatManager.Register<NameTagHandler>(new string[] { "n", "name" });
    // ChatManager.Register<AchievementTagHandler>(new string[] { "a", "achievement" });
    // ChatManager.Register<GlyphTagHandler>(new string[] { "g", "glyph" });
    /// </summary>
    public override void Load()
    {
        ChatManager.Register<BoldTagHandler>(["b", "bold"]);
        ChatManager.Register<EmojiTagHandler>(["e", "emoji"]); // or :
        ChatManager.Register<ItalicsTagHandler>(["italics"]); // avoid i to prevent item tag conflict
        ChatManager.Register<LinkTagHandler>(["l", "link"]);
        ChatManager.Register<MentionTagHandler>(["mention"]); // or @
        ChatManager.Register<ModIconTagHandler>(["m", "mod"]);
        ChatManager.Register<PlayerIconTagHandler>(["p", "player"]);
        ChatManager.Register<UploadTagHandler>(["u", "upload"]); // or #
        ChatManager.Register<UnderlineTagHandler>(["underline"]); // avoid u to prevent upload tag conflict
    }

    public override void PostSetupContent()
    {
        // Extra
        // Safe cache filled once mods are loaded
        //ModMetaCache.InitializeCache(); // build once, after mods are loaded
    }
}
