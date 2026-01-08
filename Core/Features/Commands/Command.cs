using Terraria.ModLoader;

namespace ChatPlus.Core.Features.Commands;
/// <summary>
/// A command structure that represents a chat command.
/// Can be used to define commands for the chat system.
/// </summary>
/// <param name="Name">The entire command name, e.g "/help"</param>
/// <param name="Description"></param>
/// <param name="Mod"></param>
public readonly record struct Command(string Name, string Description = null, Mod Mod = null);
