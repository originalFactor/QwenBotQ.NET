using QwenBotQ.NET.OneBot.Models;

namespace QwenBotQ.NET.OneBot.Core;

public partial class OneBot
{
    public async Task GetStrangerInfoAsync(long userId, Func<ApiResponse<GetStrangerInfoData>, Task> callback)
    {
        await CallAsync(
            "get_stranger_info",
            new GetStrangerInfoParams { UserId = userId },
            callback
        );
    }
}
