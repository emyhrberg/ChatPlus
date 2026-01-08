using ChatPlus.Core.UI;

namespace ChatPlus.Core.Features.Commands;
public class CommandState : BaseState<Command>
{
    public CommandState() : base(new CommandPanel(), new DescriptionPanel<Command>())
    {
    }
}