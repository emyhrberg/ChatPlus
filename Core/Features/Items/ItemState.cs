using ChatPlus.Core.UI;

namespace ChatPlus.Core.Features.Items;

public class ItemState : BaseState<ItemEntry>
{
    public ItemState() : base(new ItemPanel(), new DescriptionPanel<ItemEntry>())
    {
    }
}
