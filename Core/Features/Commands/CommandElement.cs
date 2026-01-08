using System;
using ChatPlus.Common;
using ChatPlus.Core.Chat;
using ChatPlus.Core.Features.ModIcons;
using ChatPlus.Core.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;
using static ChatPlus.Common.Configs.Config;

namespace ChatPlus.Core.Features.Commands;
/// <summary>
/// Represents a chat command element in the UI.
/// </summary>
public class CommandElement : BaseElement<Command>
{
    public Command _command;

    public CommandElement(Command command) : base(command)
    {
        _command = command;
    }
    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);

        Main.chatText = _command.Name; // Replace chat text with the command name

        // Set caret pos to last pos
        int chatLength = Main.chatText.Length;
        HandleChatSystem.SetCaretPos(chatLength);
    }
    protected override void DrawListElement(SpriteBatch sb)
    {
        var dims = GetDimensions();
        var pos = dims.Position();

        // Draw mod tag
        string tag = "";
        if (_command.Mod != null)
        {
            tag = ModIconTagHandler.GenerateTag(_command.Mod.Name);
        }
        else
        {
            var dest = new Rectangle((int)pos.X + 5, (int)(pos.Y + 4), (int)20, (int)24);
            sb.Draw(Ass.TerrariaIcon.Value, dest, Color.White);
        }

        Utils.DrawBorderString(sb, tag, pos += new Vector2(8, 4), Color.White);
        //Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, tag, pos.X - 8, pos.Y + 6, Color.White, Color.Black, Vector2.Zero, 1.0f);

        // Draw command name
        TextSnippet[] snip = [new TextSnippet(_command.Name)];
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, snip, pos + new Vector2(26, 1), 0f, Vector2.Zero, Vector2.One, out _);
    }

    protected override void DrawGridElement(SpriteBatch sb)
    {
        var dims = GetDimensions();
        var pos = dims.Position();

        // Draw mod tag
        string tag = "";
        if (_command.Mod != null)
        {
            tag = ModIconTagHandler.GenerateTag(_command.Mod.Name);
        }
        else
        {
            var dest = new Rectangle((int)pos.X + 6, (int)(pos.Y + 4), (int)20, (int)24);
            sb.Draw(Ass.TerrariaIcon.Value, dest, Color.White);
        }

        Utils.DrawBorderString(sb, tag, pos += new Vector2(7, 5), Color.White);
        //Utils.DrawBorderStringFourWay(sb, FontAssets.MouseText.Value, tag, pos.X - 8, pos.Y + 6, Color.White, Color.Black, Vector2.Zero, 1.0f);
    }
}
