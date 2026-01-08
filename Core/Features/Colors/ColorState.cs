using ChatPlus.Core.UI;

namespace ChatPlus.Core.Features.Colors;
public class ColorState : BaseState<ColorEntry>
{
    public ColorState() : base(new ColorPanel(), new DescriptionPanel<ColorEntry>())
    {
    }
}
