using System.Collections.Generic;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.Colors;
internal class ColorManager : ModSystem
{
    public static List<ColorEntry> Colors { get; private set; } = new List<ColorEntry>();

    public override void PostSetupContent()
    {
        Colors.Clear();

        void AddHex(string hex, string name, string description)
        {
            // e.g. [c/32FF82:Your text]
            string tagPrefix = $"[c/{hex.TrimStart('#')}:";
            Colors.Add(new ColorEntry(tagPrefix, hex, name, description));
        }

        // Status message colors
        AddHex("#FFFFFF", "Player chat", "Player chat messages.");
        AddHex("#32FF82", "Event begin", "When most events begin.");
        AddHex("#AF4BFF", "Invasion/Boss", "When an invasion begins, or when a boss is defeated or summoned.");
        AddHex("#E11919", "Player death", "When a player dies.");
        AddHex("#FF1919", "NPC/Pet death", "When a town NPC or pet dies.");
        AddHex("#327DFF", "NPC/Pet arrived", "When a town NPC or pet arrives.");
        AddHex("#FFF014", "Status message", "General status messages.");
        AddHex("#FF00A0", "Party", "When an NPC throws a Party.");

        // Neutral colors
        AddHex("#000000", "Black", "Black");
        AddHex("#404040", "Dark Gray", "Dark Gray");
        AddHex("#808080", "Gray", "Gray");
        AddHex("#C0C0C0", "Light Gray", "Light Gray");

        // Warm tones
        AddHex("#DC143C", "Crimson", "Crimson");
        AddHex("#FFA500", "Orange", "Orange");
        AddHex("#FFD700", "Gold", "Gold");
        AddHex("#A52A2A", "Brown", "Brown");

        // Cool tones
        AddHex("#00FF00", "Lime", "Lime");
        AddHex("#008080", "Teal", "Teal");
        AddHex("#00FFFF", "Cyan", "Cyan");
        AddHex("#87CEEB", "Sky Blue", "Sky Blue");
        AddHex("#4B0082", "Indigo", "Indigo");
        AddHex("#EE82EE", "Violet", "Violet");
        AddHex("#FFC0CB", "Pink", "Pink");
        AddHex("#40E0D0", "Turquoise", "Turquoise");
    }
}
