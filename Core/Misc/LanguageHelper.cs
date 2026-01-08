namespace ChatPlus.Core.Misc;

/// <summary>
/// Used for quick access to localized text in the mod.
/// Example: Loc.Get("SomeKey", arg1, arg2)
/// Uses the key "Mods.ChatPlus.SomeKey" in the localization files.
/// </summary>
public static class Loc
{
    public static string Get(string key, params object[] args)
    {
        if (Terraria.Localization.Language.Exists($"Mods.ChatPlus.{key}"))
        {
            return Terraria.Localization.Language.GetTextValue($"Mods.ChatPlus.{key}", args);
        }
        else
        {
            key = key.Replace("Mods.ChatPlus", string.Empty);
            return key;
        }
    }
}
