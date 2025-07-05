using QwenBotQ.NET.OneBot.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace QwenBotQ.NET.OneBot.Core;

public partial class OneBot
{
    private async Task CallAsync<ParamType>(string action, ParamType args, long echo = 0)
        where ParamType : BaseParams
    {
        var data = JsonConvert.SerializeObject(new ApiCall<ParamType>
        {
            Action = action,
            Params = args,
            Echo = echo
        });
        _logger.LogInformation($"Sending API Call: {data}");
        try
        {
            await _client.SendAsync(data);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling action {action}: {ex.Message}");
        }
    }

    private async Task CallAsync<ParamType, RespType>
        (string action, ParamType args, Func<RespType, Task> callback)
        where ParamType : BaseParams
        where RespType : ApiResponse
    {
        var echo = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await CallAsync(action, args, echo);
        _logger.LogInformation($"Waiting for response for action {action} with echo {echo}");
        _api_waiting[echo] = new Tuple<Type, Func<ApiResponse, Task>>
        (
            typeof(RespType),
            async (response) =>
            {
                if (response is RespType resp)
                {
                    _logger.LogInformation($"Received response for action {action} with {echo}: {JsonConvert.SerializeObject(resp)}");
                    await callback(resp);
                }
                else
                {
                    _logger.LogWarning($"Unexpected response type for action {action} with {echo}: {response.GetType().Name}");
                }
            }
        );
    }
}
