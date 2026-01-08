using ChatPlus.Core.UI;
using Microsoft.Xna.Framework.Graphics;

namespace ChatPlus.Core.Features.PlayerIcons;

public class PlayerIconState : BaseState<PlayerIcon>
{
    public PlayerIconState() : base(new PlayerIconPanel(), new DescriptionPanel<PlayerIcon>()) { }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }
}
