using ChatPlus.Core.UI;

namespace ChatPlus.Core.Features.Uploads;

public class UploadState : BaseState<Upload>
{
    public UploadState() : base(new UploadPanel(), new DescriptionPanel<Upload>())
    {
    }
}
