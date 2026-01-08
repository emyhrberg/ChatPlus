using ChatPlus.Core.UI;

namespace ChatPlus.Core.Features.Emojis;
public class EmojiState : BaseState<Emoji>
{
    public EmojiState() : base(new EmojiPanel(), new DescriptionPanel<Emoji>())
    {
    }
}
