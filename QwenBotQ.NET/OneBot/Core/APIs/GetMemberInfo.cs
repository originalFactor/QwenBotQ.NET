using QwenBotQ.NET.OneBot.Models;

namespace QwenBotQ.NET.OneBot.Core
{
    internal partial class OneBot
    {
        public async Task GetGroupMemberInfoAsync(
            long groupId,
            long userId,
            Func<ApiResponse, Task> callback)
        {
            await CallAsync<GetGroupMemberInfoParams>(
                "get_group_member_info",
                new GetGroupMemberInfoParams { GroupId = groupId, UserId = userId },
                typeof(ApiResponse<GetGroupMemberInfoData>),
                callback
            );
        }
    }
}
