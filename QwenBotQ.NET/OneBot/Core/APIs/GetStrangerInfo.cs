using QwenBotQ.NET.OneBot.Models;

namespace QwenBotQ.NET.OneBot.Core
{
    internal partial class OneBot
    {
        public async Task GetStrangerInfoAsync(long userId, Func<ApiResponse, Task> callback)
        {
            await CallAsync<GetStrangerInfoParams>(
                "get_stranger_info",
                new GetStrangerInfoParams { UserId = userId },
                typeof(ApiResponse<GetStrangerInfoData>),
                callback
            );
        }
    }
}
