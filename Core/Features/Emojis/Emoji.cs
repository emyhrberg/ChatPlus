using System.Collections.Generic;

namespace ChatPlus.Core.Features.Emojis;

/// <summary>
/// An emoji that can be sent and rendered in chat.
/// </summary>
/// <param name="FilePath">e.g "/ChatPlus/Assets/Emojis/12345.png"</param> 
/// <param name="Description"> e.g "happy_face"</param>
/// <param name="Tag">e.g "[e:happy_face]</param>
/// <param name="Synonyms"> A single emoji can have multiple search synonyms </param>
public readonly record struct Emoji(string FilePath, string Description, string Tag, List<string> Synonyms);
