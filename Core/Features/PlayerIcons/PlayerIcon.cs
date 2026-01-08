namespace ChatPlus.Core.Features.PlayerIcons;

/// <summary>
/// Represents a player entry that can be inserted into chat as a tag and whose head icon can be drawn.
/// </summary>
/// <param name="Tag">The chat tag, e.g. "[p:PlayerName]"</param>
/// <param name="PlayerIndex">Index in Main.player array.</param>
/// <param name="PlayerName">Display name (may change if renamed)</param>
public readonly record struct PlayerIcon(string Tag, int PlayerIndex, string PlayerName);
