using QwenBotQ.NET.OneBot.Models;

namespace QwenBotQ.NET.OneBot.Core;

public partial class OneBot
{
    public async Task GetGroupMemberInfoAsync(
        long groupId,
        long userId,
        Func<ApiResponse<GetGroupMemberInfoData>, Task> callback)
    {
        await CallAsync(
            "get_group_member_info",
            new GetGroupMemberInfoParams { GroupId = groupId, UserId = userId },
            callback
        );
    }
}
