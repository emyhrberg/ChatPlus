namespace ChatPlus.Core.Features.Items;

/// <summary>
/// Represents an item color for the item window and [i:ID] tag insertion.
/// </summary>
/// <param name="ID"> ID like 0, 1, 2, 3, etc...</param>
/// <param name="Tag">Chat tag like [i:ID] that renders the item icon</param>
/// <param name="DisplayName">ItemEntry display name like "Zenith" </param>
public readonly record struct ItemEntry(int ID, string Tag, string DisplayName);
