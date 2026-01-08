using Microsoft.Xna.Framework.Graphics;

namespace ChatPlus.Core.Features.Uploads;

/// <summary>
/// An uploaded image/content that can be sent and rendered as a tag in chat.
/// </summary>
/// <param name="Tag">"[u:FileName] </param>
/// <param name="FileName">FileName</param>
/// <param name="FullFilePath">UniqueDraw:/Folder/FileName.png</param>
/// <param name="Asset"></param>
public readonly record struct Upload(string Tag, string FileName, string FullFilePath, Texture2D Texture);
