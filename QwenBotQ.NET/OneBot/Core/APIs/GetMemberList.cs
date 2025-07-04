using QwenBotQ.NET.OneBot.Models;

namespace QwenBotQ.NET.OneBot.Core
{
    internal partial class OneBot
    {
        public async Task GetMemberListAsync(long groupId, Func<ApiResponse, Task> callback)
        {
            await CallAsync<GetGroupMemberListParams>(
                "get_group_member_list",
                new GetGroupMemberListParams { GroupId = groupId },
                typeof(ApiResponse<List<GetGroupMemberInfoData>>),
                callback
            );
        }
    }
}