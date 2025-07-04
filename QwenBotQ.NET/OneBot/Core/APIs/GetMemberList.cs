using QwenBotQ.NET.OneBot.Models;

namespace QwenBotQ.NET.OneBot.Core
{
    public partial class OneBot
    {
        public async Task GetMemberListAsync(long groupId, Func<MultiApiResponse<GetGroupMemberInfoData>, Task> callback)
        {
            await CallAsync(
                "get_group_member_list",
                new GetGroupMemberListParams { GroupId = groupId },
                callback
            );
        }
    }
}