namespace ChatPlus.Core.Features.Glyphs;

/// <summary>
/// Represents a glyph that can be used in the chat UI.
/// </summary>
/// <param name="Tag">e.g "[g:0]" </param>
/// <param name="Description"> e.g "X" </param>
public readonly record struct Glyph(string Tag, string Description);
