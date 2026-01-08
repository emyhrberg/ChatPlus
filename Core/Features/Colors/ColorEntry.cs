namespace ChatPlus.Core.Features.Colors;

/// <summary>
/// Represents a color instance displaying information about a color.
/// </summary>
/// <param name="Tag">  GenerateEmojiTag: [c/32FF82]  </param>
/// <param name="Hex">  Hex: Hexadecimal color code, e.g. "#32FF82" </param>
/// <param name="Name"> Name: Short name, e.g. "Event color" </param>
/// <param name="Description">  Description: When most events begin. </param>
public readonly record struct ColorEntry(string Tag, string Hex, string Name, string Description);
