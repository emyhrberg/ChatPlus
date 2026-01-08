using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace ChatPlus.Core.Features.Scrollbar;

public class ChatScrollState : UIState
{
    public ChatScrollbar chatScrollbar;
    public ChatScrollList chatScrollList;

    public ChatScrollState()
    {
        // Initialize chat scroll UI elements
        chatScrollList = new ChatScrollList();
        chatScrollbar = new ChatScrollbar();
        chatScrollList.SetScrollbar(chatScrollbar);
        Append(chatScrollList);
        Append(chatScrollbar);
    }

    public override void Draw(SpriteBatch sb)
    {
        base.Draw(sb);
    }
}
