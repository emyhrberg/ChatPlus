using System.Collections.Generic;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader;

namespace ChatPlus.Core.Features.Glyphs;

internal class GlyphManager : ModSystem
{
    public static List<Glyph> Glyphs { get; private set; } = [];

    private static readonly List<string> descriptions =
[
    "A",                                            // 0
    "B",                                            // 1
    "X",                                            // 2
    "Y",                                            // 3
    "Back",                                         // 4
    "Start",                                        // 5
    "Left Shoulder Button",                         // 6
    "Right Shoulder Button",                        // 7
    "Left Trigger",                                 // 8
    "Right Trigger",                                // 9
    "Left Stick",                                   // 10
    "Right Stick",                                  // 11
    "Undefined Stick",                              // 12
    "D-pad Right",                                  // 13
    "D-pad Left",                                   // 14
    "D-pad Down",                                   // 15
    "D-pad Up",                                     // 16
    "Left Stick Left",                              // 17
    "Left Stick Right",                             // 18
    "Left Stick Up",                                // 19
    "Left Stick Down",                              // 20
    "Right Stick Left",                             // 21
    "Right Stick Right",                            // 22
    "Right Stick Up",                               // 23
    "Right Stick Down",                             // 24
    "Left Stick Wiggling Left and Right"            // 25
];

    public override void PostSetupContent()
    {
        Glyphs.Clear();

        // Vanilla glyphs: 0..25 (26 total)
        for (int i = 0; i < 26; i++)
        {
            string tag = GlyphTagHandler.GenerateTag(i);
            string desc = descriptions[i];
            Glyphs.Add(new Glyph(tag, desc));
        }
    }
}
